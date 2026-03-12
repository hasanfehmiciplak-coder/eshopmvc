namespace EShopMVC.Domain.Events
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