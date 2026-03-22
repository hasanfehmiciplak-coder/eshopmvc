using EShopMVC.Domain.Base;
using EShopMVC.Models;
using EShopMVC.Models.Fraud;
using EShopMVC.Modules.Fraud.Models;
using EShopMVC.Modules.Orders.Domain.Entities;
using EShopMVC.Modules.Orders.Domain.Enums;
using EShopMVC.Modules.Orders.Domain.Events;
using EShopMVC.Modules.Orders.Domain.Logs;
using EShopMVC.Modules.Orders.Domain.Refunds;
using EShopMVC.Shared.Domain;
using System;
using RefundStatus = EShopMVC.Modules.Orders.Domain.Enums.RefundStatus;

namespace EShopMVC.Modules.Orders.Domain.Entities
{
    public class Order : BaseEntity, IAggregateRoot
    {
        private Order()
        {
        }

        private readonly List<OrderItem> _items = new();

        public string? IpAddress { get; private set; }

        public bool CancelRequested { get; private set; }

        public IReadOnlyCollection<OrderItem> Items => _items;

        public string UserId { get; private set; }

        public decimal TotalPrice { get; private set; }

        public string CustomerName { get; set; }

        public string CustomerEmail { get; set; }

        public DateTime OrderDate { get; private set; }

        public OrderStatus Status { get; private set; }

        public bool IsPaid { get; set; }

        public int Product { get; set; }

        public ICollection<FraudFlag> FraudFlags { get; private set; }
        public string Address { get; set; }

        public List<PaymentLog> PaymentLogs { get; set; }

        public bool RefundOverrideEnabled { get; private set; }
        public string? RefundOverrideNote { get; private set; }
        public DateTime? RefundOverrideAt { get; private set; }
        public string? RefundOverrideByUserId { get; private set; }
        public DateTime CreatedAt { get; set; }

        public DateTime? PaidAt { get; private set; }

        public List<Refund> PartialRefunds { get; private set; } = new();

        public void AddItem(int productId, int quantity, decimal price)
        {
            var item = new OrderItem(productId, quantity, price);

            _items.Add(item);

            RecalculateTotal();
        }

        private void RecalculateTotal()
        {
            TotalPrice = _items.Sum(x => x.Price * x.Quantity);
        }

        public void RequestRefund(decimal amount)
        {
            if (amount > TotalPrice)
                throw new Exception("Refund amount cannot exceed total price");
        }

        public void MarkAsPaid()
        {
            if (Status != OrderStatus.Pending)
                throw new InvalidOperationException("Order cannot be marked as paid.");

            Status = OrderStatus.Paid;
            IsPaid = true;
            PaidAt = DateTime.UtcNow;
        }

        public void MarkAsShipped()
        {
            if (Status != OrderStatus.Paid && Status != OrderStatus.Processing)
                throw new InvalidOperationException("Order must be paid before shipping.");

            Status = OrderStatus.Shipped;
        }

        public void Complete()
        {
            if (Status != OrderStatus.Shipped)
                throw new InvalidOperationException("Order must be shipped before completion.");

            Status = OrderStatus.Completed;
        }

        public void Cancel()
        {
            if (Status == OrderStatus.Shipped || Status == OrderStatus.Completed)
                throw new InvalidOperationException("Order cannot be cancelled.");

            Status = OrderStatus.Cancelled;
        }

        public PaymentLog ReceivePayment(decimal amount, string provider, string idempotencyKey)
        {
            if (Status != OrderStatus.Pending)
                throw new InvalidOperationException("Payment cannot be processed.");

            var paymentLog = new PaymentLog(Id, amount, provider, idempotencyKey);

            MarkAsPaid();

            AddDomainEvent(new PaymentReceivedEvent(Id, amount));

            return paymentLog;
        }

        public void MarkPaymentFailed()
        {
            Status = OrderStatus.PaymentFailed;
        }

        public void SetStatus(OrderStatus status)
        {
            Status = status;
        }

        public Order(string userId, decimal totalPrice)
        {
            UserId = userId;
            TotalPrice = totalPrice;
            OrderDate = DateTime.UtcNow;
            Status = OrderStatus.Pending;
        }

        public void MarkAsFraudReview()
        {
            Status = OrderStatus.FraudReview;
        }

        public void SetIpAddress(string ip)
        {
            IpAddress = ip;
        }

        public void MarkAsBlocked()
        {
            Status = OrderStatus.Blocked;
        }

        public void EnableRefundOverride(string note, string userId)
        {
            if (string.IsNullOrWhiteSpace(note))
                throw new ArgumentException("Override note is required");

            RefundOverrideEnabled = true;
            RefundOverrideNote = note;
            RefundOverrideAt = DateTime.UtcNow;
            RefundOverrideByUserId = userId;
        }

        public bool HasActiveHighFraud =>
         FraudFlags != null &&
         FraudFlags.Any(f =>
        !f.IsResolved &&
        f.Severity == FraudSeverity.High);

        public void MarkAsRefunded()
        {
            Status = OrderStatus.Refunded;
        }

        public void RequestCancel()
        {
            CancelRequested = true;
        }

        private readonly List<RefundRequest> _refundRequests = new();

        public IReadOnlyCollection<RefundRequest> RefundRequests => _refundRequests;

        public RefundRequest RequestRefund(string reason)
        {
            if (!IsPaid)
                throw new InvalidOperationException("Order not paid");

            if (_refundRequests.Any(r => r.Status == RefundStatus.Pending))
                throw new InvalidOperationException("Refund already requested");

            var refund = new RefundRequest(Id, reason);

            _refundRequests.Add(refund);

            return refund;
        }

        private readonly List<Refund> _refunds = new();

        public IReadOnlyCollection<Refund> Refunds => _refunds;

        public Refund CreateRefund(int orderItemId, int quantity, decimal amount)
        {
            var refund = new Refund(orderItemId, quantity, amount);

            _refunds.Add(refund);

            return refund;
        }
    }
}