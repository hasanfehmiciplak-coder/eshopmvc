namespace EShopMVC.Modules.Orders.Domain.Entities
{
    public class OrderTimeline
    {
        public int Id { get; private set; }

        public int OrderId { get; private set; }

        public string Event { get; private set; }

        public string? Description { get; private set; }

        public DateTime CreatedAt { get; private set; }

        private OrderTimeline()
        { }

        public OrderTimeline(int orderId, string @event, string? description = null)
        {
            OrderId = orderId;
            Event = @event;
            Description = description;
            CreatedAt = DateTime.UtcNow;
        }
    }
}