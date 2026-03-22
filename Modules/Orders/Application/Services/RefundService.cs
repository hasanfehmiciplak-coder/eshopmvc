using EShopMVC.Infrastructure.Data;
using EShopMVC.Modules.Orders.Domain.Enums;
using EShopMVC.Modules.Orders.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace EShopMVC.Modules.Orders.Application.Services
{
    public class RefundService
    {
        private readonly AppDbContext _context;
        private readonly RefundPolicy _policy;

        public RefundService(AppDbContext context, RefundPolicy policy)
        {
            _context = context;
            _policy = policy;
        }

        // REFUND BAŞLAT
        public async Task CreateRefundAsync(int refundId)
        {
            var refund = await _context.Refunds
                .FirstOrDefaultAsync(x => x.Id == refundId);

            if (refund == null)
                throw new Exception("Refund bulunamadı");

            try
            {
                // payment gateway simülasyonu
                refund.Status = RefundStatus.Success;
                refund.NextRetryAt = null;
            }
            catch
            {
                refund.Status = RefundStatus.Failed;
                refund.NextRetryAt = DateTime.UtcNow.AddMinutes(10);
            }

            await _context.SaveChangesAsync();
        }

        // RETRY
        public async Task RetryRefundAsync(int refundId)
        {
            var refund = await _context.Refunds
                .FirstOrDefaultAsync(x => x.Id == refundId);

            if (refund == null)
                throw new Exception("Refund bulunamadı");

            if (refund.Status != RefundStatus.Failed &&
                refund.Status != RefundStatus.PermanentFailed)
            {
                throw new Exception("Refund retry edilemez");
            }

            await CreateRefundAsync(refundId);
        }

        public async Task RequestRefund(int orderId, string reason)
        {
            var order = await _context.Orders
                .Include(o => o.RefundRequests)
                .FirstAsync(o => o.Id == orderId);

            if (!_policy.CanRefund(order))
                throw new Exception("Refund not allowed");

            order.RequestRefund(reason);

            await _context.SaveChangesAsync();
        }
    }
}