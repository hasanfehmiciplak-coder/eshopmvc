using EShopMVC.Shared.Domain;

namespace EShopMVC.Modules.Orders.Refund
{
    public class RefundTimeline : BaseEntity
    {
        private RefundTimeline()
        { }

        public int RefundId { get; private set; }

        public string Event { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public RefundTimeline(int refundId, string @event)
        {
            RefundId = refundId;
            Event = @event;
            CreatedAt = DateTime.UtcNow;
        }
    }
}