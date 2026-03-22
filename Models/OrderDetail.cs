using EShopMVC.Models;
using EShopMVC.Modules.Orders.Domain.Entities;

namespace EShopMVC.Modules.Orders.Domain
{
    public class OrderDetail
    {
        public int Id { get; set; }

        // Foreign Keys
        public int OrderId { get; set; }

        public Order Order { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}