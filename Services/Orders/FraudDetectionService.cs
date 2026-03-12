using EShopMVC.Data;
using EShopMVC.Domain.Events;
using EShopMVC.Infrastructure.Data;
using EShopMVC.Models;
using EShopMVC.Models.Fraud;
using EShopMVC.Modules.Fraud.Models;
using Microsoft.EntityFrameworkCore;

namespace EShopMVC.Modules.Fraud.Services
{
    public class FraudDetectionService
    {
        private readonly AppDbContext _context;

        public FraudDetectionService(AppDbContext context)
        {
            _context = context;
        }

        public async Task CheckRefundFraud(int orderId)
        {
            var isFraud = true; // örnek kontrol

            if (isFraud)
            {
                var fraudFlag = new FraudFlag
                {
                    OrderId = orderId,
                    Reason = FraudReason.RefundTooFast,
                    CreatedAt = DateTime.UtcNow
                };

                _context.FraudFlags.Add(fraudFlag);

                // DOMAIN EVENT
                var fraudEvent = new FraudDetectedEvent(orderId);

                // şimdilik sadece log gibi kullanıyoruz
                Console.WriteLine($"Fraud detected for Order {fraudEvent.OrderId}");

                await _context.SaveChangesAsync();
            }
        }

        public async Task CheckRulesAsync(int orderId)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
                return;

            var payment = await _context.PaymentLogs
                .Where(p => p.OrderId == orderId)
                .OrderBy(p => p.CreatedAt)
                .FirstOrDefaultAsync();

            var refund = await _context.RefundLogs
                .Where(r => r.OrderId == orderId)
                .OrderBy(r => r.CreatedAt)
                .FirstOrDefaultAsync();

            // RULE 1
            if (payment != null && refund != null)
            {
                var diff = refund.CreatedAt - payment.CreatedAt;

                if (diff.TotalMinutes < 10)
                {
                    await CreateFraudFlag(orderId, FraudReason.RefundTooFast);
                }
            }

            // RULE 2
            var refundCount = await _context.RefundLogs
                .CountAsync(r => r.OrderId == orderId);

            if (refundCount >= 3)
            {
                await CreateFraudFlag(orderId, FraudReason.MultipleRefunds);
            }
        }

        private async Task CreateFraudFlag(int orderId, FraudReason reason)
        {
            var exists = await _context.FraudFlags
                .AnyAsync(f => f.OrderId == orderId && f.Reason == reason);

            if (exists)
                return;

            var fraud = new FraudFlag
            {
                OrderId = orderId,
                Reason = reason,
                CreatedAt = DateTime.UtcNow
            };

            _context.FraudFlags.Add(fraud);

            await _context.SaveChangesAsync();
        }
    }
}