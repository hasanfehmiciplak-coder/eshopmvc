using EShopMVC.Models;
using global::EShopMVC.Models;

namespace EShopMVC.Helpers
{
    public static class OrderStatusHelper
    {
        public static string BadgeClass(OrderStatus status)
        {
            return status switch
            {
                //OrderStatus.Beklemede => "bg-secondary",
                //OrderStatus.Hazırlanıyor => "bg-warning text-dark",
                //OrderStatus.KargoyaVerildi => "bg-info text-dark",
                //OrderStatus.TeslimEdildi => "bg-success",
                //OrderStatus.IptalEdildi => "bg-danger",
                //_ => "bg-dark"
            };
        }
    }
}