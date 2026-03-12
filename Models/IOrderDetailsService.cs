using EShopMVC.Areas.Admin.ViewModels.Orders;

namespace EShopMVC.Models
{
    public interface IOrderDetailsService
    {
        Task<OrderDetailViewModel?> GetOrderDetailsAsync(int orderId);
    }
}