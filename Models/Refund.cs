namespace EShopMVC.Models
{
    public class Refund
    {
        public Guid Id { get; set; }

        public RefundStatus Status { get; set; }

        public int RetryCount { get; set; }

        public int MaxRetryCount { get; set; } = 5;

        public DateTime? NextRetryAt { get; set; }

        public int OrderId { get; set; }
        public int OrderItemId { get; set; }

        public int Quantity { get; set; }
        public decimal Amount { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
    }
}