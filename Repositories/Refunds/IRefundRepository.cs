using EShopMVC.Modules.Orders.Domain.Entities;

namespace EShopMVC.Repositories.Refunds
{
    public interface IRefundRepository
    {
        Task<Refund> GetAsync(int refundId);

        Task UpdateAsync(Refund refund);

        Task<int> DeleteCompletedOlderThanAsync(DateTime olderThan);
    }
}