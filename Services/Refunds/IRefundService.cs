namespace EShopMVC.Services.Refunds
{
    public interface IRefundService
    {
        Task CreateRefundAsync(Guid refundId);

        Task RetryRefundAsync(Guid refundId);

        Task UndoRefundAsync(
            Guid refundId,
            string reason,
            string? performedBy
        );

        //Task ApproveRefundAsync(
        //    int partialRefundId,
        //    string? approvedBy
        //);
    }
}