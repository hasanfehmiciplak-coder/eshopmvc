using EShopMVC.Infrastructure.Data;
using EShopMVC.Models;
using EShopMVC.Models.Fraud;
using EShopMVC.Modules.Fraud.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EShopMVC.Modules.Fraud.Services
{
    public class FraudPatternService : Controller
    {
        private readonly AppDbContext _context;

        public FraudPatternService(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task DetectIpPattern()
        {
            var suspiciousIps = await _context.PaymentLogs
                .Where(x => x.IpAddress != null)
                .GroupBy(x => x.IpAddress)
                .Where(g => g.Select(x => x.OrderId).Distinct().Count() >= 3)
                .Select(g => g.Key)
                .ToListAsync();

            foreach (var ip in suspiciousIps)
            {
                var orders = await _context.PaymentLogs
                    .Where(x => x.IpAddress == ip)
                    .Select(x => x.OrderId)
                    .Distinct()
                    .ToListAsync();

                foreach (var orderId in orders)
                {
                    var exists = await _context.FraudFlags
                        .AnyAsync(x =>
                            x.OrderId == orderId &&
                            x.RuleCode == "IP_PATTERN");

                    if (!exists)
                    {
                        _context.FraudFlags.Add(new FraudFlag
                        {
                            OrderId = orderId,
                            RuleCode = "IP_PATTERN",
                            Description = "Aynı IP adresinden çoklu ödeme denemesi.",
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}