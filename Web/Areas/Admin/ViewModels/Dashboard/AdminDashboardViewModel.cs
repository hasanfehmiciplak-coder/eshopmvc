using EShopMVC.Areas.Admin.ViewModels.Orders;

namespace EShopMVC.Areas.Admin.ViewModels.Dashboard
{
    public class AdminDashboardViewModel
    {
        public AdminDashboardViewModel()
        {
            FraudLast7Days = new();
            RefundLast7Days = new();
        }

        public int ActiveFraudCount { get; set; }
        public int HighFraudCount { get; set; }
        public int TodayRefundCount { get; set; }
        public decimal TotalRefundAmount { get; set; }

        public int FailedPaymentCount { get; set; }
        public int HighRiskUserCount { get; set; }
        public int MediumRiskUserCount { get; set; }
        public int LowRiskUserCount { get; set; }

        public List<DailyCountVM> FraudLast7Days { get; set; } = new();
        public List<DailyCountVM> RefundLast7Days { get; set; } = new();

        public List<OrderTimelineItemVM> RecentActivities { get; set; } = new();
    }
}