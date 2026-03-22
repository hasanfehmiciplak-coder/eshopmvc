using EShopMVC.Modules.Orders.Domain.Entities;
using EShopMVC.Shared.Domain;

namespace EShopMVC.Models
{
    public class Cart : BaseEntity
    {
        public int Id { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public ICollection<CartItem> CartItems { get; set; }

        public IReadOnlyCollection<CartItem> Items => _items;
        private readonly List<CartItem> _items = new();

        public void AddItem(int productId, int quantity)
        {
            var item = Items.FirstOrDefault(i => i.ProductId == productId);

            if (item == null)
            {
                _items.Add(new CartItem(Id, productId, quantity));
            }
            else
            {
                item.IncreaseQuantity(quantity);
            }
        }
    }
}