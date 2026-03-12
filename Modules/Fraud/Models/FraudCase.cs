namespace EShopMVC.Modules.Fraud.Models
{
    public class FraudCase
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        public FraudCaseStatus Status { get; set; } = FraudCaseStatus.Open;

        public string? AssignedToUserId { get; set; }

        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ClosedAt { get; set; }
    }

    public enum FraudCaseStatus
    {
        Open,
        Investigating,
        Resolved,
        Rejected,
        Closed
    }
}