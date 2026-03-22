using EShopMVC.Modules.Orders.Domain.Entities;
using EShopMVC.Modules.Orders.Domain.Enums;

namespace EShopMVC.Modules.Orders.Domain.Refunds
{
    public class Refund
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

        public string? RequestedByUserId { get; set; }
        public string? ApprovedByUserId { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public ICollection<Refund> Refunds { get; set; } = new List<Refund>();

        public DateTime? RejectedAt { get; set; }
        public string? RejectedByUserId { get; set; }

        public bool RetryLimitMailSent { get; set; }

        public bool PdfDownloaded { get; set; }
        public DateTime? PdfDownloadedAt { get; set; }

        public DateTime? CancelledAt { get; set; }
    }
}