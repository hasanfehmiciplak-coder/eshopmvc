using EShopMVC.Modules.Orders.Domain.Entities;

namespace EShopMVC.Web.ViewModels
{
    public class UserDashboardViewModel
    {
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
        public Order? LastOrder { get; set; }
    }
}