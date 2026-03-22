using EShopMVC.Infrastructure.Data;
using EShopMVC.Modules.Orders.Domain.Entities;
using EShopMVC.Modules.Orders.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EShopMVC.Repositories.Refunds
{
    public class RefundRepository : IRefundRepository
    {
        private readonly AppDbContext _context;

        public RefundRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Refund> GetAsync(int refundId)
        {
            return await _context.Refunds
                .FirstOrDefaultAsync(x => x.Id == refundId);
        }

        public async Task UpdateAsync(Refund refund)
        {
            _context.Refunds.Update(refund);
            await _context.SaveChangesAsync();
        }

        public async Task<int> DeleteCompletedOlderThanAsync(DateTime olderThan)
        {
            var oldRefunds = _context.Refunds
                .Where(x => x.Status == RefundStatus.Completed
                         && x.CreatedAt < olderThan);

            _context.Refunds.RemoveRange(oldRefunds);

            return await _context.SaveChangesAsync();
        }
    }
}