namespace EShopMVC.Domain.Events
{
    public class PaymentReceivedEvent : DomainEvent
    {
        public int OrderId { get; set; }

        public PaymentReceivedEvent(int orderId)
        {
            OrderId = orderId;
        }
    }
}