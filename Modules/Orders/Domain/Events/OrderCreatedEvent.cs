using EShopMVC.Shared.Events;
using MediatR;

namespace EShopMVC.Modules.Orders.Domain.Events
{
    public class OrderCreatedEvent : DomainEvent, INotification
    {
        public int OrderId { get; }

        public OrderCreatedEvent(int orderId)
        {
            OrderId = orderId;
        }
    }
}