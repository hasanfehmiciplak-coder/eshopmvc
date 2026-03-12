using EShopMVC.Models;
using EShopMVC.Models.Fraud;

namespace EShopMVC.Areas.Admin.ViewModels
{
    public class OrderListItemVM
    {
        public int Id { get; set; }
        public string UserEmail { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalPrice { get; set; }
        public OrderStatus Status { get; set; }

        public FraudSeverity? MaxFraudSeverity { get; set; } // 👈
    }
}