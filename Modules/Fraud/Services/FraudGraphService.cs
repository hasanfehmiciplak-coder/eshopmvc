using EShopMVC.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Order = EShopMVC.Modules.Orders.Domain.Entities.Order;

public class FraudGraphService
{
    private readonly AppDbContext _context;

    public FraudGraphService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<object> GetGraphData()
    {
        var data = await _context.PaymentLogs
            .Where(x => x.IpAddress != null)
            .Select(x => new
            {
                User = _context.Users
                    .Where(u => u.Id == x.Order.UserId)
                    .Select(u => u.Email)
                    .FirstOrDefault(),

                Order = x.OrderId,
                Ip = x.IpAddress
            })
            .Take(50)
            .ToListAsync();

        return data;
    }
}