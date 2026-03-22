namespace EShopMVC.Modules.Orders.Domain.Enums
{
    public enum OrderStatus
    {
        Pending = 0,
        Paid = 1,
        Processing = 2,
        Shipped = 3,
        Completed = 4,
        Cancelled = 5,
        Refunded = 6,
        Approved = 7,
        Rejected = 11,

        //PaymentFailed = 7,
        //Delivered = 8,
        PartialRefund = 9,

        PaymentFailed = 10,
        Blocked = 12,
        CancelRequested = 13,   // 🔥 ekle
        CancelRejected = 14,
        Delivered = 15, // 🔥 ekle
        FraudReview = 16
    }
}