namespace EShopMVC.Areas.Admin.ViewModels.Orders
{
    public class OrderTimelineItemVM
    {
        public DateTime Date { get; set; }
        public string Type { get; set; } // Payment / Refund / Fraud
        public string Title { get; set; }
        public string Description { get; set; }

        public int? EntityId { get; set; }   // FraudId, RefundId, PaymentLogId
        public string? EntityType { get; set; } // "Payment" | "Refund" | "Fraud"
    }
}