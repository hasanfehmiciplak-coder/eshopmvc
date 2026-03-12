using EShopMVC.Infrastructure.Data;
using EShopMVC.Models;
using EShopMVC.Models.Fraud;
using EShopMVC.Models.TimeLine;
using EShopMVC.Modules.Fraud.Models;
using Microsoft.EntityFrameworkCore;

namespace EShopMVC.Modules.Fraud.Services
{
    public class FraudScoreService
    {
        private readonly AppDbContext _context;
        private readonly OrderTimelineService _timelineService;

        public FraudScoreService(AppDbContext context, OrderTimelineService timelineService)
        {
            _context = context;
            _timelineService = timelineService;
        }

        public async Task<int> CalculateScore(int orderId)
        {
            int score = 0;

            var paymentLogs = await _context.PaymentLogs
                .Where(x => x.OrderId == orderId)
                .ToListAsync();

            var failCount = paymentLogs
                .Count(x => x.PaymentStatus != "SUCCESS");

            if (failCount >= 3)
                score += 20;

            if (failCount >= 5)
                score += 40;

            var hasFraudFlag = await _context.FraudFlags
                .AnyAsync(x => x.OrderId == orderId);

            if (hasFraudFlag)
                score += 30;

            var refundCount = await _context.PartialRefunds
                .CountAsync(x => x.OrderId == orderId);

            if (refundCount > 0)
                score += 10;

            var distinctCards = paymentLogs
                .Select(x => x.CardLast4)
                .Distinct()
                .Count();

            if (distinctCards > 1)
                score += 20;

            return score;
        }

        public async Task<bool> CheckAutoBlock(int orderId)
        {
            var score = await CalculateScore(orderId);

            if (score < 80)
                return false;

            var order = await _context.Orders.FindAsync(orderId);

            if (order == null)
                return false;

            order.Status = OrderStatus.Blocked;

            _context.FraudFlags.Add(new FraudFlag
            {
                OrderId = order.Id,
                RuleCode = "AUTO_BLOCK_HIGH_RISK",
                Description = "Sipariş yüksek risk nedeniyle otomatik bloklandı.",
                CreatedAt = DateTime.UtcNow
            });

            await _timelineService.AddAsync(
                order.Id,
                TimelineEventType.Fraud,
                "🚫 Sipariş yüksek risk nedeniyle otomatik bloklandı",
                "SYSTEM");

            await _context.SaveChangesAsync();

            return true;
        }

        public async Task CheckVelocityRule(int orderId)
        {
            var fiveMinutesAgo = DateTime.UtcNow.AddMinutes(-5);

            var failCount = await _context.PaymentLogs
                .Where(x =>
                    x.OrderId == orderId &&
                    x.PaymentStatus != "SUCCESS" &&
                    x.CreatedAt >= fiveMinutesAgo)
                .CountAsync();

            if (failCount < 3)
                return;

            var exists = await _context.FraudFlags
                .AnyAsync(x =>
                    x.OrderId == orderId &&
                    x.RuleCode == "PAYMENT_VELOCITY");

            if (exists)
                return;

            _context.FraudFlags.Add(new FraudFlag
            {
                OrderId = orderId,
                RuleCode = "PAYMENT_VELOCITY",
                Description = "Kısa sürede çok fazla ödeme denemesi.",
                CreatedAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
        }

        public async Task<int> CalculateRefundScore(int orderId)
        {
            var refunds = await _context.PartialRefunds
                .Where(x => x.OrderId == orderId)
                .CountAsync();

            if (refunds >= 3)
                return 30;

            return 0;
        }

        public async Task<int> CalculateIpScore(int orderId)
        {
            var ip = await _context.Orders
                .Where(x => x.Id == orderId)
                .Select(x => x.IpAddress)
                .FirstOrDefaultAsync();

            if (ip == null)
                return 0;

            var count = await _context.Orders
                .Where(x => x.IpAddress == ip)
                .CountAsync();

            if (count > 5)
                return 20;

            return 0;
        }
    }
}