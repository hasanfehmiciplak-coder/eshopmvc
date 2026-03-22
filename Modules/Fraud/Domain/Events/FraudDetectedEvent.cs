using EShopMVC.Shared.Events;

namespace EShopMVC.Modules.Fraud.Domain.Events
{
    public class FraudDetectedEvent : DomainEvent
    {
        public int OrderId { get; set; }

        public FraudDetectedEvent(int orderId)
        {
            OrderId = orderId;
        }
    }
}