using EShopMVC.Infrastructure.Data;
using EShopMVC.Models.Fraud;
using EShopMVC.Modules.Fraud.Models;

namespace EShopMVC.Modules.Fraud.Services
{
    public class FraudCaseService
    {
        private readonly AppDbContext _context;

        public FraudCaseService(AppDbContext context)
        {
            _context = context;
        }

        public async Task CreateCaseAsync(
            int orderId,
            string userId,
            FraudSeverity severity)
        {
            var fraudCase = new FraudCase
            {
                OrderId = orderId,
                Status = Models.FraudCaseStatus.Open,
                CreatedAt = DateTime.UtcNow
            };

            _context.FraudCases.Add(fraudCase);

            await _context.SaveChangesAsync();
        }
    }
}