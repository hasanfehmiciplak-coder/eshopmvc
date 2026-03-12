using EShopMVC.Infrastructure.Data;
using EShopMVC.Models;
using EShopMVC.Modules.Fraud.Models;
using EShopMVC.Modules.Orders.Models;
using Microsoft.EntityFrameworkCore;

namespace EShopMVC.Modules.Fraud.Services
{
    public class FraudAutoBlockService
    {
        private readonly AppDbContext _context;

        public FraudAutoBlockService(AppDbContext context)
        {
            _context = context;
        }

        public async Task HandleHighRiskAsync(Order order, int riskScore)
        {
            if (riskScore < 90)
                return;

            // Order hold
            order.Status = OrderStatus.FraudReview;

            // IP ban
            if (!string.IsNullOrEmpty(order.IpAddress))
            {
                var exists = await _context.BannedIps
                    .AnyAsync(x => x.IpAddress == order.IpAddress);

                if (!exists)
                {
                    _context.BannedIps.Add(new BannedIp
                    {
                        IpAddress = order.IpAddress,
                        Reason = "High fraud risk"
                    });
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}