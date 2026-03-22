using EShopMVC.Modules.Orders.Domain.Entities;
using EShopMVC.Modules.Orders.Domain.Enums;

namespace EShopMVC.Models
{
    public static class OrderStatusCalculator
    {
        public static OrderStatus Calculate(Order order)
        {
            if (order.Items.All(x => x.RefundedQuantity == 0))
                return OrderStatus.Paid; // veya Completed

            if (order.Items.All(x => x.RefundedQuantity == x.Quantity))
                return OrderStatus.Refunded;

            return OrderStatus.PartialRefund;
        }
    }
}