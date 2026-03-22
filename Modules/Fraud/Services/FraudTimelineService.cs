using EShopMVC.Infrastructure.Data;
using EShopMVC.Models.TimeLine;
using EShopMVC.Modules.Orders.Domain;

namespace EShopMVC.Modules.Fraud.Services
{
    public class FraudTimelineService
    {
        private readonly AppDbContext _context;

        public FraudTimelineService(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddRiskScoreEvent(int orderId, int score)
        {
            var timeline = new OrderTimeline
            {
                OrderId = orderId,
                EventType = TimelineEventType.FraudScoreCalculated,
                Description = $"Fraud risk score hesaplandı: {score}",
                CreatedAt = DateTime.UtcNow
            };

            _context.OrderTimelines.Add(timeline);

            await _context.SaveChangesAsync();
        }

        public async Task AddFraudFlagEvent(int orderId, string rule)
        {
            var timeline = new OrderTimeline
            {
                OrderId = orderId,
                EventType = TimelineEventType.FraudFlagCreated,
                Description = $"Fraud flag oluşturuldu: {rule}",
                CreatedAt = DateTime.UtcNow
            };

            _context.OrderTimelines.Add(timeline);

            await _context.SaveChangesAsync();
        }
    }
}