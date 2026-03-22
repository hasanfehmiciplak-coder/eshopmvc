using EShopMVC.Infrastructure.Data;
using EShopMVC.Models;
using EShopMVC.Models.Fraud;
using EShopMVC.Modules.Fraud.Models;
using EShopMVC.Modules.Fraud.Repositories;
using EShopMVC.Modules.Orders.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EShopMVC.Modules.Fraud.Services
{
    public class FraudService : IFraudService
    {
        private readonly IFraudFlagRepository _fraudFlagRepository;
        private readonly ILogger<FraudService> _logger;
        private readonly AppDbContext _context;

        public FraudService(
            IFraudFlagRepository fraudFlagRepository,
            ILogger<FraudService> logger,
            AppDbContext context)
        {
            _fraudFlagRepository = fraudFlagRepository;
            _logger = logger;
            _context = context;
        }

        public FraudFlag? EvaluateRefund(Order order, Orders.Domain.Entities.Refund refund)
        {
            throw new NotImplementedException();
        }

        // 🔥 5️⃣ BURASI
        public void FlagRefund(FraudFlag flag)
        {
            // Aynı flag varsa tekrar ekleme
            if (_fraudFlagRepository.Exists(flag.RefundId, flag.Reason))
                return;

            _fraudFlagRepository.Add(flag);

            _logger.LogWarning(
                "FraudFlag created. RefundId={RefundId}, Reason={Reason}",
                flag.RefundId,
                flag.Reason);
        }

        public async Task ResolveRefundFraudAsync(int orderId)
        {
            var flags = await _context.FraudFlags
                .Where(x => x.OrderId == orderId && x.IsActive)
                .ToListAsync();

            foreach (var flag in flags)
            {
                flag.IsActive = false;
                flag.ResolvedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }
    }
}