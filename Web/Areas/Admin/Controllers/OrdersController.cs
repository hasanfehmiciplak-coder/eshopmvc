using EShopMVC.Areas.Admin.ViewModels.Orders;
using EShopMVC.Infrastructure.Data;
using EShopMVC.Models;
using EShopMVC.Modules.Orders.Application;
using EShopMVC.Modules.Orders.Application.Services;
using EShopMVC.Modules.Orders.Domain.Entities;
using EShopMVC.Modules.Orders.Domain.Enums;
using EShopMVC.Web.Areas.Admin.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Refund = EShopMVC.Modules.Orders.Domain.Entities.Refund;

namespace EShopMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OrdersController : Controller
    {
        private readonly AppDbContext _context;
        private readonly OrderRiskService _riskService;
        private readonly CheckoutService _checkoutService;
        private readonly RefundService _refundService;

        public OrdersController(
            AppDbContext context,
            OrderRiskService riskService,
            CheckoutService checkoutService,
            RefundService refundService)
        {
            _context = context;
            _riskService = riskService;
            _checkoutService = checkoutService;
            _refundService = refundService;
        }

        // 📦 ORDER LIST
        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .Select(o => new OrderListVM
                {
                    Id = o.Id,
                    UserEmail = _context.Users
                        .Where(u => u.Id == o.UserId)
                        .Select(u => u.Email)
                        .FirstOrDefault(),
                    OrderDate = o.OrderDate,
                    TotalPrice = o.TotalPrice,
                    Status = o.Status
                })
                .ToListAsync();

            return View(orders);
        }

        // 📄 DETAILS
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);

            // ✔ NULL kontrol EN ÖNCE
            if (order == null)
                return NotFound();

            var refunds = await _context.Refunds
                .Where(r => r.OrderItemId == order.Id)
                .ToListAsync();

            var paymentLogs = await _context.PaymentLogs
                .Where(p => p.OrderId == order.Id)
                .ToListAsync();

            var riskScore = await _riskService.CalculateRiskScore(order.Id);
            ViewBag.RiskScore = riskScore;

            // ✔ TEK vm
            var vm = new OrderDetailViewModel
            {
                Order = order,
                Refunds = refunds,
                PaymentLogs = paymentLogs
            };

            return View(vm);
        }

        // ❌ CANCEL ORDER
        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return NotFound();

            order.SetStatus(OrderStatus.Cancelled);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id });
        }

        // 🚚 SHIP ORDER
        [HttpPost]
        public async Task<IActionResult> Ship(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return NotFound();

            order.SetStatus(OrderStatus.Shipped);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id });
        }

        // ✅ COMPLETE ORDER
        [HttpPost]
        public async Task<IActionResult> Complete(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
                return NotFound();

            order.SetStatus(OrderStatus.Completed);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id });
        }

        // 💳 PAYMENT TEST
        [HttpPost]
        public async Task<IActionResult> CompletePayment(int orderId)
        {
            await _checkoutService.ProcessPayment(
                orderId,
                "TX123",
                "CreditCard",
                "SUCCESS"
            );

            return RedirectToAction(nameof(Details), new { id = orderId });
        }
    }
}