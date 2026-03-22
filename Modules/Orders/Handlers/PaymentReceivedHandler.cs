using EShopMVC.Domain.Interfaces;
using EShopMVC.Infrastructure.Data;
using EShopMVC.Models.TimeLine;
using EShopMVC.Modules.Orders.Domain.Events;

namespace EShopMVC.Modules.Orders.Handlers
{
    public class PaymentReceivedHandler : IDomainEventHandler<PaymentReceivedEvent>
    {
        private readonly AppDbContext _context;

        public PaymentReceivedHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task Handle(PaymentReceivedEvent domainEvent)
        {
            var timeline = new OrderTimeline
            {
                OrderId = domainEvent.OrderId,
                EventType = TimelineEventType.PaymentReceived,
                Details = $"Payment received: {domainEvent.Amount} ₺",
                CreatedAt = DateTime.UtcNow
            };

            _context.OrderTimelines.Add(timeline);

            await _context.SaveChangesAsync();
        }
    }
}