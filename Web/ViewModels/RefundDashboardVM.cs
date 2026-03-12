namespace EShopMVC.Web.ViewModels
{
    public class RefundDashboardVM
    {
        public int TotalRefundCount { get; set; }
        public decimal TotalRefundAmount { get; set; }

        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public int PendingCount { get; set; }

        public int RetryCount { get; set; }
        public int RetryLimitExceededCount { get; set; }

        public int SlaViolatedCount { get; set; }

        public List<DailyRefundStat> Last7Days { get; set; } = new();

        public int PdfAvailableCount { get; set; }
    }

    public class DailyRefundStat
    {
        public DateTime Date { get; set; }
        public int Count { get; set; }
        public decimal Amount { get; set; }
    }
}