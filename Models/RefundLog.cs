using EShopMVC.Modules.Orders.Models;
using EShopMVC.Modules.Payments.Models;

namespace EShopMVC.Models
{
    public class RefundLog
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; }

        public string PaymentTransactionId { get; set; }

        public decimal Amount { get; set; }

        public RefundStatus Status { get; set; }

        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int? PaymentLogId { get; set; }
        public PaymentLog? PaymentLog { get; set; }

        public string RawResponse { get; set; }

        public int? OrderItemId { get; set; }     // 👈 HANGİ ÜRÜN
        public int Quantity { get; set; }         // 👈 KAÇ ADET

        public string AdminUserId { get; set; }
        public int PartialRefundId { get; set; }

        public string Provider { get; set; } // iyzico

        // Add navigation property for PartialRefund
        public PartialRefund PartialRefund { get; set; }
        public string Reason { get; internal set; }
    }
}