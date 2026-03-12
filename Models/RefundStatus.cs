namespace EShopMVC.Models
{
    public enum RefundStatus
    {
        Pending = 0,
        PendingApproval = 1,
        Completed = 2,
        Failed = 3,
        PermanentFailed = 4,
        Success = 5,
        Cancelled = 6
    }
}