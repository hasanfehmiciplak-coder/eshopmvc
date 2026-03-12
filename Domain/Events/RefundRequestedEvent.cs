namespace EShopMVC.Domain.Events
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