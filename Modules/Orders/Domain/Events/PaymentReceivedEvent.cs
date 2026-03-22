using EShopMVC.Shared.Events;

namespace EShopMVC.Modules.Orders.Domain.Events
{
    public class PaymentReceivedEvent : DomainEvent
    {
        public int OrderId { get; }
        public decimal Amount { get; }

        public PaymentReceivedEvent(int orderId, decimal amount)
        {
            OrderId = orderId;
            Amount = amount;
        }
    }
}