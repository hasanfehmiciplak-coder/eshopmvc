using EShopMVC.Models;
using EShopMVC.Modules.Orders.Domain.Enums;

namespace EShopMVC.Areas.Admin.ViewModels
{
    public class AdminLastOrderViewModel
    {
        public int Id { get; set; }
        public string UserEmail { get; set; }
        public DateTime Date { get; set; }
        public decimal Total { get; set; }
        public OrderStatus Status { get; set; }
    }
}