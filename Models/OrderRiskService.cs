using EShopMVC.Infrastructure.Data;
using EShopMVC.Modules.Orders.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EShopMVC.Models
{
    public class OrderRiskService
    {
        private readonly AppDbContext _context;

        public OrderRiskService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int> CalculateRiskScore(int orderId)
        {
            int score = 0;

            var failCount = await _context.PaymentLogs
                .Where(x => x.OrderId == orderId && x.Status != "SUCCESS")
                .CountAsync();

            if (failCount >= 3)
                score += 20;

            if (failCount >= 5)
                score += 40;

            var hasFraudFlag = await _context.FraudFlags
                .AnyAsync(x => x.OrderId == orderId);

            if (hasFraudFlag)
                score += 30;

            var refundCount = await _context.Refunds
                .Where(x => x.OrderItemId == orderId)
                .CountAsync();

            if (refundCount > 0)
                score += 10;

            return score;
        }

        public async Task<List<(Order order, int score)>> GetRiskyOrders()
        {
            var orders = await _context.Orders
                .OrderByDescending(x => x.OrderDate)
                .Take(50)
                .ToListAsync();

            var result = new List<(Order order, int score)>();

            foreach (var order in orders)
            {
                var score = await CalculateRiskScore(order.Id);

                if (score >= 40)
                    result.Add((order, score));
            }

            return result
                .OrderByDescending(x => x.score)
                .Take(5)
                .ToList();
        }
    }
}