using EShopMVC.Domain.Entities;
using EShopMVC.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

public class OutboxProcessor : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public OutboxProcessor(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();

            var context = scope.ServiceProvider
                .GetRequiredService<AppDbContext>();

            var messages = await context.OutboxMessages
                //.Where(x => x.ProcessedOn == null && x.RetryCount < 5)
                .Where(x => x.ProcessedOn == null)
                .OrderBy(x => x.OccurredOn)
                .Take(20)
                .ToListAsync(stoppingToken);

            foreach (var message in messages)
            {
                try
                {
                    Console.WriteLine($"Processing event {message.Type}");

                    // burada event handler çağrılabilir

                    message.ProcessedOn = DateTime.UtcNow;
                }
                catch (Exception ex)
                {
                    //message.RetryCount++;

                    //message.Error = ex.Message;
                }
            }

            await context.SaveChangesAsync(stoppingToken);

            await Task.Delay(5000, stoppingToken);
        }
    }
}