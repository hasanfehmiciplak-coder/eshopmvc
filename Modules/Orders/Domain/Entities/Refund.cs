using EShopMVC.Modules.Orders.Domain.Enums;
using EShopMVC.Shared.Domain;

namespace EShopMVC.Modules.Orders.Domain.Entities
{
    public class Refund : BaseEntity
    {
        private Refund()
        { }

        public int Id { get; set; }

        public int OrderId { get; set; }

        public Order Order { get; set; }

        public int OrderItemId { get; private set; }
        public OrderItem OrderItem { get; private set; }

        public int Quantity { get; private set; }

        public RefundStatus Status { get; set; }

        public int RetryCount { get; set; }

        public ICollection<Refund> ChildRefunds { get; set; }

        public DateTime? NextRetryAt { get; set; }

        public DateTime CreatedAt { get; private set; }
        public decimal Amount { get; private set; }

        public int? RefundId { get; private set; }   // retry chain
        public Refund ParentRefund { get; private set; }

        public decimal RefundAmount { get; private set; }

        public string? Reason { get; private set; }

        public Refund(int orderItemId, int quantity, decimal amount)
        {
            OrderItemId = orderItemId;
            Quantity = quantity;
            Amount = amount;
            Status = RefundStatus.Pending;
            CreatedAt = DateTime.UtcNow;
        }

        public void MarkSuccess()
        {
            Status = RefundStatus.Success;
        }

        public void MarkFailed()
        {
            Status = RefundStatus.Failed;
            RetryCount++;
        }

        public void ScheduleRetry(DateTime nextRetry)
        {
            NextRetryAt = nextRetry;
        }
    }
}