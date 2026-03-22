using EShopMVC.Shared.Events;

namespace EShopMVC.Modules.Orders.Domain.Events
{
    public class RefundRequestedEvent : DomainEvent
    {
        public int OrderId { get; set; }

        public RefundRequestedEvent(int orderId)
        {
            OrderId = orderId;
        }
    }
}