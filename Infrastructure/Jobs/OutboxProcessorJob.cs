using EShopMVC.Infrastructure.Data;
using EShopMVC.Shared.EventBus;
using EShopMVC.Shared.Outbox;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace EShopMVC.Infrastructure.Jobs
{
    public class OutboxProcessorJob
    {
        private readonly AppDbContext _context;
        private readonly IEventBus _eventBus;

        public OutboxProcessorJob(AppDbContext context, IEventBus eventBus)
        {
            _context = context;
            _eventBus = eventBus;
        }

        public async Task ProcessAsync()
        {
            var messages = await _context.OutboxMessages
                .Where(x => x.ProcessedOn == null)
                .Take(20)
                .ToListAsync();

            foreach (var message in messages)
            {
                var type = Type.GetType(message.Type);

                var domainEvent = JsonSerializer.Deserialize(message.Payload, type);

                await _eventBus.PublishAsync((dynamic)domainEvent);

                message.ProcessedOn = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }
    }
}