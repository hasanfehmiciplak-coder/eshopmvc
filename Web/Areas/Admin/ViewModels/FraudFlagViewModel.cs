using EShopMVC.Models.Fraud;

namespace EShopMVC.Areas.Admin.ViewModels
{
    public class FraudFlagViewModel
    {
        public int Id { get; set; }

        public string RuleCode { get; set; }
        public string Description { get; set; }

        public FraudSeverity Severity { get; set; }

        public bool IsResolved { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}