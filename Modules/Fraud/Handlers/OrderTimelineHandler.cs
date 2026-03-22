using EShopMVC.Infrastructure.Data;
using EShopMVC.Modules.Orders.Events;
using EShopMVC.Modules.Orders.Domain.Logs;
using MediatR;
using EShopMVC.Modules.Orders.Domain.Events;

public class OrderTimelineHandler : INotificationHandler<OrderCreatedEvent>
{
    private readonly AppDbContext _context;

    public OrderTimelineHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task Handle(OrderCreatedEvent notification,
                             CancellationToken cancellationToken)
    {
        var log = new OrderLog
        {
            OrderId = notification.OrderId,
            EventType = "OrderCreated",
            Description = "Order created"
        };

        _context.OrderLogs.Add(log);

        await _context.SaveChangesAsync();
    }
}