using EShopMVC.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EShopMVC.Modules.Fraud.Services
{
    public class FraudRuleEngine
    {
        private readonly AppDbContext _context;

        public FraudRuleEngine(AppDbContext context)
        {
            _context = context;
        }

        public async Task<int> CalculateRiskScoreAsync(int orderId)
        {
            var rules = await _context.FraudRules
                .Where(x => x.IsActive)
                .ToListAsync();

            int score = 0;

            foreach (var rule in rules)
            {
                switch (rule.RuleType)
                {
                    case "HighAmount":
                        // örnek kontrol
                        score += rule.RiskScore;
                        break;

                    case "MultipleRefund":
                        score += rule.RiskScore;
                        break;
                }
            }

            return score;
        }
    }
}