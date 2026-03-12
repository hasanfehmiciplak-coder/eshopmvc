using EShopMVC.Infrastructure.Data;
using EShopMVC.Models.Fraud;
using EShopMVC.Modules.Fraud.Models;
using Microsoft.EntityFrameworkCore;

public class UserFraudService
{
    private readonly AppDbContext _context;

    public UserFraudService(AppDbContext context)
    {
        _context = context;
    }

    public async Task UpdateUserScore(string userId)
    {
        var orders = await _context.Orders
            .Where(o => o.UserId == userId)
            .ToListAsync();

        int score = 0;

        foreach (var order in orders)
        {
            var failCount = await _context.PaymentLogs
                .CountAsync(x =>
                    x.OrderId == order.Id &&
                    x.PaymentStatus != "SUCCESS");

            if (failCount >= 3)
                score += 20;

            if (failCount >= 5)
                score += 40;

            var fraud = await _context.FraudFlags
                .AnyAsync(x => x.OrderId == order.Id);

            if (fraud)
                score += 30;

            var refund = await _context.PartialRefunds
                .AnyAsync(x => x.OrderId == order.Id);

            if (refund)
                score += 10;
        }

        string level =
            score >= 80 ? "High" :
            score >= 40 ? "Medium" :
            "Low";

        var existing = await _context.UserFraudScores
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (existing == null)
        {
            existing = new UserFraudScore
            {
                UserId = userId
            };

            _context.UserFraudScores.Add(existing);
        }

        existing.Score = score;
        existing.RiskLevel = level;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task<int> GetUserRiskScore(int orderId)
    {
        var order = await _context.Orders
            .Where(x => x.Id == orderId)
            .Select(x => x.UserId)
            .FirstOrDefaultAsync();

        if (order == null)
            return 0;

        var score = await _context.UserFraudScores
            .Where(x => x.UserId == order)
            .Select(x => x.Score)
            .FirstOrDefaultAsync();

        return score;
    }
}