using EShopMVC.Infrastructure.Data;
using EShopMVC.Models.TimeLine;

namespace EShopMVC.Models
{
    public class OrderTimelineService
    {
        private readonly AppDbContext _context;

        public OrderTimelineService(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(
            int orderId,
            TimelineEventType eventType,
            string description,
            string performedBy)
        {
            var timeline = new OrderTimeline
            {
                OrderId = orderId,
                EventType = eventType,
                Description = string.IsNullOrWhiteSpace(description)
                ? eventType.ToString()
                : description,

                Details = "",
                PerformedByUserName = string.IsNullOrWhiteSpace(performedBy)
                ? "SYSTEM"
                : performedBy,

                CreatedAt = DateTime.UtcNow
            };

            _context.OrderTimelines.Add(timeline);

            await _context.SaveChangesAsync();
        }
    }
}