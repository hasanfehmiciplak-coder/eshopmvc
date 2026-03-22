using EShopMVC.Modules.Orders.Domain.Entities;
using EShopMVC.Infrastructure.Data;
using EShopMVC.Models;
using EShopMVC.Modules.Fraud.Services;
using EShopMVC.Modules.Orders.Domain.Events;
using EShopMVC.Modules.Orders.Domain.Logs;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Diagnostics;
using OrderCreatedEvent = EShopMVC.Modules.Orders.Domain.Events.OrderCreatedEvent;

namespace EShopMVC.Modules.Orders.Application
{
    public class CheckoutService
    {
        private readonly AppDbContext _context;
        private readonly FraudDetectionService _fraudService;
        private readonly TransactionService _transactionService;

        public CheckoutService(AppDbContext context, FraudDetectionService fraudService, TransactionService transactionService)
        {
            _context = context;
            _fraudService = fraudService;
            _transactionService = transactionService;
        }

        public async Task<Order> CreateOrderFromCart(string userId)
        {
            var stopwatch = Stopwatch.StartNew();

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
                throw new InvalidOperationException("Cart not found");

            if (!cart.CartItems.Any())
                throw new InvalidOperationException("Cart is empty");

            // ✔ ORDER OLUŞTUR

            // 🔥 TOTAL PRICE HESAPLA
            var totalPrice = cart.CartItems
                .Sum(x => x.Product.UnitPrice * x.Quantity);

            // ✔ ORDER OLUŞTUR
            var order = new Order(userId, totalPrice);

            foreach (var item in cart.CartItems)
            {
                if (item.Product == null)
                    throw new Exception("Product not loaded");

                if (item.Product.Stock < item.Quantity)
                    throw new Exception($"{item.Product.Name} için yeterli stok yok");

                // 🔻 STOK DÜŞ
                item.Product.Stock -= item.Quantity;

                // ✔ ORDER ITEM EKLE
                order.AddItem(
                    item.ProductId,
                    item.Quantity,
                    item.Product.UnitPrice
                );
            }

            _context.Orders.Add(order);

            // ✔ EVENT
            order.AddDomainEvent(new OrderCreatedEvent(order.Id));

            // 🧹 SEPET TEMİZLE
            _context.CartItems.RemoveRange(cart.CartItems);

            // ✔ PAYMENT
            var paymentLog = order.ReceivePayment(
                order.TotalPrice,
                "Stripe",
                Guid.NewGuid().ToString()
            );

            _context.PaymentLogs.Add(paymentLog);

            await _context.SaveChangesAsync();

            stopwatch.Stop();

            Log.Information("Checkout for user {UserId} took {Elapsed} ms",
                userId,
                stopwatch.ElapsedMilliseconds);

            return order;
        }

        public async Task ProcessPayment(
            int orderId,
            string status,
            string transactionId,
            string rawResponse)
        {
            await _transactionService.ExecuteAsync(async () =>
            {
                var alreadyProcessed = await _context.PaymentLogs
                    .AnyAsync(p => p.IdempotencyKey == transactionId);

                if (alreadyProcessed)
                    return;

                var order = await _context.Orders
                    .FirstAsync(o => o.Id == orderId);

                if (status == "SUCCESS")
                {
                    var paymentLog = order.ReceivePayment(
                        order.TotalPrice,
                        "Iyzico",
                        transactionId
                    );

                    _context.PaymentLogs.Add(paymentLog);
                }
                else
                {
                    order.MarkPaymentFailed();
                }
            });
        }
    }
}