using EShopMVC.Models;

namespace EShopMVC.Modules.Fraud.Models

{
    public class UserFraudScore
    {
        public int Id { get; set; }
        public string UserId { get; set; }

        public ApplicationUser User { get; set; }

        public int TotalFraudFlags { get; set; }
        public int HighSeverityCount { get; set; }
        public int ActiveFraudCount { get; set; }

        public int TotalRefundCount { get; set; }
        public decimal TotalRefundAmount { get; set; }

        public int Score { get; set; }
        public string RiskLevel { get; set; } // Low / Medium / High

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}