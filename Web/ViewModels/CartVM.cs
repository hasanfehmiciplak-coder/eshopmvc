using EShopMVC.Modules.Orders.Domain.Entities;

namespace EShopMVC.Web.ViewModels
{
    public class CartVM
    {
        public List<CartItem> CartItems { get; set; } = new();

        public decimal TotalPrice =>
            CartItems.Sum(x => x.Product.UnitPrice * x.Quantity);
    }
}