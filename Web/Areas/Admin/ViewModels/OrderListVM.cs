using EShopMVC.Modules.Orders.Domain.Enums;

namespace EShopMVC.Web.Areas.Admin.ViewModels
{
    public class OrderListVM
    {
        public int Id { get; set; }
        public string UserEmail { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalPrice { get; set; }
        public OrderStatus Status { get; set; }
        public bool CancelRequested { get; set; }
    }
}