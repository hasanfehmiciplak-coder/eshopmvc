using EShopMVC.Modules.Orders.Models;

namespace EShopMVC.Models
{
    public static class OrderStatusCalculator
    {
        public static OrderStatus Calculate(Order order)
        {
            if (order.OrderItems.All(x => x.RefundedQuantity == 0))
                return OrderStatus.Paid; // veya Completed

            if (order.OrderItems.All(x => x.RefundedQuantity == x.Quantity))
                return OrderStatus.Refunded;

            return OrderStatus.PartialRefund;
        }
    }
}