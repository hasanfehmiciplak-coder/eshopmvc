using EShopMVC.Infrastructure.Data;
using EShopMVC.Modules.Fraud.Models;

namespace EShopMVC.Modules.Fraud.Services
{
    public class FraudAlertService
    {
        private readonly AppDbContext _context;

        public FraudAlertService(AppDbContext context)
        {
            _context = context;
        }

        public async Task CreateAlertAsync(int orderId, int riskScore)
        {
            var alert = new FraudAlert
            {
                OrderId = orderId,
                RiskScore = riskScore,
                Message = $"High risk order detected (Score: {riskScore})"
            };

            _context.FraudAlerts.Add(alert);

            await _context.SaveChangesAsync();
        }
    }
}