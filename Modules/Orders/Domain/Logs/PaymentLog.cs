using EShopMVC.Modules.Orders.Domain.Entities;
using EShopMVC.Shared.Domain;

namespace EShopMVC.Modules.Orders.Domain.Logs
{
    public class PaymentLog : BaseEntity
    {
        public int OrderId { get; private set; }
        public Order Order { get; private set; }

        public decimal Amount { get; private set; }

        public string PaymentProvider { get; private set; }

        public DateTime PaidAt { get; private set; }

        public string IdempotencyKey { get; private set; }

        public int Id { get; private set; }

        public string PaymentTransactionId { get; private set; }

        public decimal PaidAmount { get; private set; }

        public string Method { get; private set; }

        public string Status { get; private set; }
        public string IpAddress { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public string? CardLast4 { get; set; }

        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }

        public string? ConversationId { get; set; }

        public PaymentLog(int orderId, decimal amount, string method, string status)
        {
            OrderId = orderId;
            PaidAmount = amount;
            Status = status;
            PaymentTransactionId = Guid.NewGuid().ToString();
            CreatedAt = DateTime.UtcNow;
        }
    }
}

//using EShopMVC.Modules.Orders.Domain.Entities;

//namespace EShopMVC.Modules.Orders.Domain.Logs
//{
//    public class PaymentLog
//    {
//        public int Id { get; set; }

//        public int OrderId { get; set; }
//        public Order Order { get; set; } = null!;

//        public string Provider { get; set; } = null!;
//        public string PaymentStatus { get; set; } = null!;

//        public decimal PaidAmount { get; set; }

//        public string? PaymentTransactionId { get; set; }
//        public string? ConversationId { get; set; }

//        public string? ErrorCode { get; set; }
//        public string? ErrorMessage { get; set; }

//        public string? RawResponse { get; set; }
//        public string? IpAddress { get; set; }

//        public string? CardLast4 { get; set; }

//        public DateTime CreatedAt { get; set; } = DateTime.Now;

//        public string Status { get; set; } = "UNKNOWN";
//    }
//}