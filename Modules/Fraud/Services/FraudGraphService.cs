using EShopMVC.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

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
            .Include(x => x.Order)
            .ThenInclude(o => o.User)
            .Select(x => new
            {
                User = x.Order.User.Email,
                Order = x.OrderId,
                Ip = x.IpAddress
            })
            .Where(x => x.Ip != null)
            .Take(50)
            .ToListAsync();

        return data;
    }
}