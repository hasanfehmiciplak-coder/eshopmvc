using EShopMVC.Modules.Orders.Domain.Entities;
using EShopMVC.Modules.Orders.Domain.Enums;
using EShopMVC.Modules.Orders.Domain.Logs;

namespace EShopMVC.Modules.Orders.Domain.Refunds
{
    public class RefundLog
    {
        public int Id { get; private set; }

        public int OrderId { get; private set; }
        public Order Order { get; set; }
        public string UserEmail { get; set; }

        public string PaymentTransactionId { get; set; }

        public decimal Amount { get; set; }

        public RefundStatus Status { get; set; }

        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }

        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
        public int? PaymentLogId { get; set; }
        public PaymentLog? PaymentLog { get; set; }
        public string Reason { get; private set; }

        public string RawResponse { get; set; }

        public int? OrderItemId { get; set; }

        public OrderItem OrderItem { get; set; }
        public int Quantity { get; set; }

        public string AdminUserId { get; set; }
        public int PartialRefundId { get; set; }

        public string Provider { get; set; } // iyzico

        public Refund Refund { get; set; }

        public RefundLog(int orderId, string reason)
        {
            OrderId = orderId;
            Reason = reason;
            CreatedAt = DateTime.UtcNow;
        }
    }
}