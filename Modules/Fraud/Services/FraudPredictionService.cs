using EShopMVC.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

public class FraudPredictionService
{
    private readonly AppDbContext _context;

    public FraudPredictionService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<double> PredictOrderFraud(int orderId)
    {
        double score = 0;

        var failCount = await _context.PaymentLogs
            .CountAsync(x =>
                x.OrderId == orderId &&
                x.PaymentStatus != "SUCCESS");

        if (failCount >= 3)
            score += 0.25;

        var velocity = await _context.FraudFlags
            .AnyAsync(x =>
                x.OrderId == orderId &&
                x.RuleCode == "PAYMENT_VELOCITY");

        if (velocity)
            score += 0.20;

        var refund = await _context.PartialRefunds
            .AnyAsync(x => x.OrderId == orderId);

        if (refund)
            score += 0.15;

        var ipPattern = await _context.FraudFlags
            .AnyAsync(x =>
                x.OrderId == orderId &&
                x.RuleCode == "IP_PATTERN");

        if (ipPattern)
            score += 0.20;

        var order = await _context.Orders
            .FirstOrDefaultAsync(x => x.Id == orderId);

        if (order != null)
        {
            var userRisk = await _context.UserFraudScores
                .FirstOrDefaultAsync(x => x.UserId == order.UserId);

            if (userRisk != null && userRisk.RiskLevel == "High")
                score += 0.20;
        }

        return Math.Min(score, 1);
    }
}