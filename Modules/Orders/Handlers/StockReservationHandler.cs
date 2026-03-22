using EShopMVC.Infrastructure.Data;
using EShopMVC.Modules.Orders.Domain.Entities;
using EShopMVC.Modules.Orders.Domain.Events;
using EShopMVC.Modules.Orders.Events;
using Microsoft.EntityFrameworkCore;

public class StockReservationHandler
{
    private readonly AppDbContext _context;

    public StockReservationHandler(AppDbContext context)
    {
        _context = context;
    }

    public async Task Handle(OrderCreatedEvent notification)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .FirstAsync(o => o.Id == notification.OrderId);

        foreach (var item in order.Items)
        {
            var inventory = await _context.Inventories
                .FirstAsync(x =>
                    x.ProductId == item.ProductId &&
                    x.VariantId == item.VariantId);

            if (inventory.AvailableQuantity < item.Quantity)
                throw new Exception("Insufficient stock");

            inventory.Reserve(item.Quantity);

            _context.StockReservations.Add(new StockReservation
            {
                OrderId = order.Id,
                ProductId = item.ProductId,
                VariantId = (int)item.VariantId,
                Quantity = item.Quantity
            });
        }

        await _context.SaveChangesAsync();
    }
}