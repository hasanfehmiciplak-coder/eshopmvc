using EShopMVC.Infrastructure.Data;
using EShopMVC.Models;
using EShopMVC.Services;
using Microsoft.Extensions.Options;

namespace EShopMVC.Modules.Payments.Services
{
    public class IyzicoRefundService : IPaymentRefundGateway
    {
        private readonly IOptions<IyzicoOptions> _options;
        private readonly AppDbContext _context;

        public IyzicoRefundService(
            IOptions<IyzicoOptions> options,
            AppDbContext context)
        {
            _options = options;
            _context = context;
        }

        public Task<bool> RefundAsync(Refund refund)
        {
            // şimdilik dummy
            return Task.FromResult(true);
        }
    }
}