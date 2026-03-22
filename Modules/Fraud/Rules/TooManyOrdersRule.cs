using EShopMVC.Infrastructure.Data;
using EShopMVC.Models.Fraud;
using EShopMVC.Modules.Fraud.Models;
using EShopMVC.Modules.Fraud.Rules;
using EShopMVC.Modules.Orders.Domain.Entities;

public class TooManyOrdersRule : IFraudRule
{
    private readonly AppDbContext _context;

    public TooManyOrdersRule(AppDbContext context)
    {
        _context = context;
    }

    public FraudReason? Check(Order order)
    {
        var count = _context.Orders
            .Count(o => o.UserId == order.UserId &&
                        o.OrderDate > DateTime.UtcNow.AddMinutes(-10));

        if (count > 3)
        {
            return FraudReason.TooManyOrders;
        }

        return null;
    }
}