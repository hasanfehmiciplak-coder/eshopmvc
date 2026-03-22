namespace EShopMVC.Modules.Orders.Domain.Enums
{
    public enum RefundStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2,
        PendingApproval = 3,
        Success = 4,
        Completed = 5,
        Failed = 6,
        PermanentFailed = 7,
        Cancelled = 8,
        Processing = 9
    }
}