using EShopMVC.Models.Fraud;
using EShopMVC.Modules.Orders.Models;

namespace EShopMVC.Modules.Fraud.Models
{
    public class FraudFlag
    {
        public int Id { get; set; }

        public int? OrderId { get; set; }
        public Order Order { get; set; }

        public Guid? RefundId { get; set; }

        public FraudReason Reason { get; set; }
        public FraudSeverity Severity { get; set; }

        public string RuleCode { get; set; }
        public string Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsResolved { get; set; } = false;
        public string ResolvedByUserId { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string ResolutionNote { get; set; }
        public bool IsActive { get; internal set; }

        public FraudCaseStatus CaseStatus { get; set; } = FraudCaseStatus.Open;
    }
}