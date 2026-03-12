namespace EShopMVC.Areas.Admin.ViewModels
{
    public class FraudSimulationVM
    {
        public decimal OrderAmount { get; set; }

        public int RefundCount { get; set; }

        public string IpAddress { get; set; }

        public int RiskScore { get; set; }

        public List<string> TriggeredRules { get; set; } = new();
    }
}