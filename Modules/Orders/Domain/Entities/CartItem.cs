using EShopMVC.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace EShopMVC.Modules.Orders.Domain.Entities
{
    public class CartItem
    {
        public int Id { get; set; }

        public int CartId { get; private set; }

        public int ProductId { get; set; }

        public int VariantId { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        public string ProductName { get; set; }

        public string Size { get; set; }

        public string Color { get; set; }

        public string ImageUrl { get; set; }

        public Cart? Cart { get; set; }

        public Product? Product { get; set; }

        public CartItem(int cartId, int productId, int quantity)
        {
            CartId = cartId;
            ProductId = productId;
            Quantity = quantity;
        }

        public void IncreaseQuantity(int quantity)
        {
            Quantity += quantity;
        }
    }
}