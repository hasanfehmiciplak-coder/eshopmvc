using EShopMVC.Infrastructure.Data;
using EShopMVC.Models.Fraud;
using EShopMVC.Modules.Fraud.Models;
using EShopMVC.Modules.Orders.Domain.Entities;
using Microsoft.EntityFrameworkCore;

public class FraudRuleService : IFraudRuleService
{
    private readonly AppDbContext _context;

    public FraudRuleService(AppDbContext context)
    {
        _context = context;
    }

    public async Task EvaluateAsync(Order order)
    {
        // RULE_01: Aynı siparişte 2+ iade
        var refundCount = await _context.Refunds
            .CountAsync(x => x.OrderItemId == order.Id);

        if (refundCount >= 2)
        {
            await AddFlag(
                order,
                "RULE_01",
                FraudReason.MultipleRefundsSameOrder,
                "Aynı sipariş için birden fazla iade talebi.");
        }

        // RULE_02: Kullanıcı son 30 günde 3+ iade
        var userRefundCount = await _context.Refunds
            .Include(x => x.OrderItem)
            .CountAsync(x =>

                x.OrderItem.Order.UserId == order.UserId &&
                x.CreatedAt >= DateTime.Today.AddDays(-30));

        if (userRefundCount >= 3)
        {
            await AddFlag(
                order,
                "RULE_02",
                FraudReason.FrequentUserRefunds,
                "Kullanıcı son 30 günde çok sayıda iade talep etti.");
        }

        // RULE_03: İade oranı %50+
        if (order.TotalPrice > 0)
        {
            var refunded = await _context.Refunds
                .Where(x => x.OrderItemId == order.Id)
                .SumAsync(x => x.Amount);

            if (refunded / order.TotalPrice >= 0.5m)
            {
                await AddFlag(
                    order,
                    "RULE_03",
                    FraudReason.HighRefundRatio,
                    "Sipariş tutarının %50’sinden fazlası iade edildi.");
            }
        }
    }

    private async Task AddFlag(
        Order order,
        string rule,
        FraudReason reason,
        string description)
    {
        var exists = await _context.FraudFlags
            .AnyAsync(x =>
                x.OrderId == order.Id &&
                x.RuleCode == rule);

        if (!exists)
        {
            _context.FraudFlags.Add(
                new FraudFlag(order.Id, rule, reason)
            );

            await _context.SaveChangesAsync();
        }
    }
}