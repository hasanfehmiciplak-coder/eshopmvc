using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using EShopMVC.Infrastructure.Data;

namespace EShopMVC.Infrastructure.BackgroundJobs
{
    public class OutboxProcessor : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public OutboxProcessor(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();

                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var messages = await db.OutboxMessages
                    .Where(x => !x.Processed)
                    .OrderBy(x => x.OccurredOn)
                    .Take(20)
                    .ToListAsync(stoppingToken);

                foreach (var message in messages)
                {
                    try
                    {
                        // Event tipini bul
                        var type = Type.GetType($"EShopMVC.Modules.Orders.Events.{message.Type}");

                        if (type == null)
                            continue;

                        // Event deserialize
                        var domainEvent = JsonSerializer.Deserialize(
                            message.Payload,
                            type
                        );

                        // burada handler çalıştırılabilir
                        // örnek: mediator.Publish(domainEvent)

                        message.Processed = true;
                        message.ProcessedOn = DateTime.UtcNow;
                    }
                    catch
                    {
                        // loglanabilir
                    }
                }

                await db.SaveChangesAsync(stoppingToken);

                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}