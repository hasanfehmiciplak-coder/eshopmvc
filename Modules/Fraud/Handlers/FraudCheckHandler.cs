using EShopMVC.Infrastructure.Data;
using EShopMVC.Modules.Fraud.Models;
using EShopMVC.Modules.Fraud.Services;
using EShopMVC.Modules.Orders.Domain.Events;
using EShopMVC.Modules.Orders.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class FraudCheckHandler : INotificationHandler<OrderCreatedEvent>
{
    private readonly AppDbContext _context;
    private readonly FraudDetectionService _fraudService;

    public FraudCheckHandler(AppDbContext context,
                             FraudDetectionService fraudService)
    {
        _context = context;
        _fraudService = fraudService;
    }

    public async Task Handle(OrderCreatedEvent notification,
                             CancellationToken cancellationToken)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .FirstAsync(o => o.Id == notification.OrderId);

        var reasons = _fraudService.Check(order);

        foreach (var reason in reasons)
        {
            var flag = new FraudFlag(order.Id, reason.ToString(), reason);
            _context.FraudFlags.Add(flag);
        }

        await _context.SaveChangesAsync();
    }
}