using RefundStatus = EShopMVC.Modules.Orders.Domain.Enums.RefundStatus;

namespace EShopMVC.Modules.Orders.Domain.Entities
{
    public class RefundRequest
    {
        public int Id { get; private set; }

        public int OrderId { get; private set; }

        public string Reason { get; private set; }

        public DateTime RequestedAt { get; private set; }

        public RefundStatus Status { get; private set; }

        public Order Order { get; private set; }

        private RefundRequest()
        { }

        public RefundRequest(int orderId, string reason)
        {
            OrderId = orderId;
            Reason = reason;
            RequestedAt = DateTime.UtcNow;
            Status = RefundStatus.Pending;
        }

        public void Approve()
        {
            Status = RefundStatus.Approved;
        }

        public void Reject()
        {
            Status = RefundStatus.Rejected;
        }
    }
}