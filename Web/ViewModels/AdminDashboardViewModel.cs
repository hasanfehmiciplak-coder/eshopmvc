using EShopMVC.Modules.Orders.Models;

namespace EShopMVC.Web.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalOrders { get; set; }
        public int TodayOrders { get; set; }
        public decimal TotalRevenue { get; set; }

        public int PendingOrders { get; set; }
        public int CancelRequestedOrders { get; set; }

        public int TotalUsers { get; set; }
        public int TotalProducts { get; set; }

        public List<Order> LastOrders { get; set; } = new();
    }
}