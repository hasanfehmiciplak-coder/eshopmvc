namespace EShopMVC.Models
{
    public enum OrderStatus
    {
        Pending = 0,
        Paid = 1,
        PaymentFailed = 2,
        Cancelled = 3,
        Shipped = 4,
        Delivered = 5,
        Completed = 6,
        Shipping = 7,
        PartialRefund = 8,
        Refunded = 9,
        Preparing = 10,
        PendingPayment = 11,
        Approved = 12,
        Rejected = 13,
        Blocked = 14,
        FraudReview = 15,
    }
}