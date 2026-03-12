using EShopMVC.Models;

namespace EShopMVC.Repositories.Refunds
{
    public interface IRefundRepository
    {
        Task<Refund> GetAsync(Guid refundId);

        Task UpdateAsync(Refund refund);

        Task<int> DeleteCompletedOlderThanAsync(DateTime olderThan);
    }
}