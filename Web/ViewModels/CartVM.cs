using EShopMVC.Modules.Orders.Models;

namespace EShopMVC.Web.ViewModels
{
    public class CartVM
    {
        public List<CartItem> CartItems { get; set; } = new();

        public decimal TotalPrice =>
            CartItems.Sum(x => x.Product.Price * x.Quantity);
    }
}