using EShopMVC.Models.Fraud;

namespace EShopMVC.Areas.Admin.ViewModels.Orders
{
    public class FraudFlagItemVM
    {
        public string RuleCode { get; set; }
        public string Description { get; set; }
        public FraudSeverity Severity { get; set; }
        public bool IsResolved { get; set; }
        public DateTime CreatedAt { get; set; }

        public int? Score { get; set; }

        public string? ResolutionNote { get; set; }
        public string? ResolvedByUserEmail { get; set; }

        // 👇 Hesaplanmış alanlar
        public int HitCount { get; set; }

        public bool IsAbuse => HitCount >= 3;

        public int Id { get; set; }              // Resolve için
        public DateTime? ResolvedAt { get; set; }

        public FraudCaseStatus CaseStatus { get; set; }

        public string SeverityClass =>
            Severity switch
            {
                FraudSeverity.High => "bg-danger",
                FraudSeverity.Medium => "bg-warning text-dark",
                FraudSeverity.Low => "bg-secondary",
                _ => "bg-light"
            };
    }
}