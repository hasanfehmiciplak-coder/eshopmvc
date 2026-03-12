namespace EShopMVC.Areas.Admin.ViewModels
{
    public class FraudMetricsVM
    {
        public int TotalAlerts { get; set; }

        public int OpenCases { get; set; }

        public int HighRiskOrders { get; set; }

        public decimal RefundAbuseRate { get; set; }
    }
}