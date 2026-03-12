using EShopMVC.Modules.Orders.Models;

namespace EShopMVC.Modules.Payments.Models
{
    public class PaymentLog
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public string Provider { get; set; } = null!;
        public string PaymentStatus { get; set; } = null!;

        public decimal PaidAmount { get; set; }

        public string? PaymentTransactionId { get; set; }
        public string? ConversationId { get; set; }

        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }

        public string? RawResponse { get; set; }
        public string? IpAddress { get; set; }

        public string? CardLast4 { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string Status { get; set; } = "UNKNOWN";
    }
}