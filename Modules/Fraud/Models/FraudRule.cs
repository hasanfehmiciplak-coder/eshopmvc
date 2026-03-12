namespace EShopMVC.Modules.Fraud.Models
{
    public class FraudRule
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string RuleType { get; set; }

        public int Threshold { get; set; }

        public int RiskScore { get; set; }

        public bool IsActive { get; set; }
    }
}