using EShopMVC.Models;
using EShopMVC.Modules.Orders.Models;

namespace EShopMVC.Modules.Payments.Models
{
    public class PartialRefund
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; }

        public int OrderItemId { get; set; }
        public OrderItem OrderItem { get; set; }

        public int Quantity { get; set; }
        public decimal RefundAmount { get; set; }

        public string? Reason { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public RefundStatus Status { get; set; } = RefundStatus.PendingApproval;

        // 👇 ONAY BİLGİLERİ (OPSİYONEL)
        public string? RequestedByUserId { get; set; }

        public string? ApprovedByUserId { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public ICollection<RefundLog> RefundLogs { get; set; } = new List<RefundLog>();

        public DateTime? RejectedAt { get; set; } // <-- Add this property
        public string? RejectedByUserId { get; set; } // <-- Add this property if needed

        public bool RetryLimitMailSent { get; set; }

        public bool PdfDownloaded { get; set; }
        public DateTime? PdfDownloadedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
    }
}