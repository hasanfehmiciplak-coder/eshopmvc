using EShopMVC.Shared.Events;

namespace EShopMVC.Modules.Orders.Events
{
    public class OrderPaidEvent : DomainEvent
    {
        public int OrderId { get; }

        public OrderPaidEvent(int orderId)
        {
            OrderId = orderId;
        }
    }
}