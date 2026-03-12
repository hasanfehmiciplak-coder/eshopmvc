using DocumentFormat.OpenXml.Office2010.Excel;
using EShopMVC.Areas.Admin.ViewModels.Orders;
using EShopMVC.Models;
using EShopMVC.Models.Fraud;
using EShopMVC.Modules.Fraud.Models;
using EShopMVC.Modules.Orders.Events;
using EShopMVC.Modules.Payments.Models;
using EShopMVC.Shared.Domain;
using Iyzipay.Model;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace EShopMVC.Modules.Orders.Models
{
    public class Order : BaseEntity
    {
        public int Id { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; } = null;

        // 🆕 Kargo bilgileri
        public string? CargoCompany { get; set; }

        public string CustomerEmail { get; set; }

        public string CustomerName { get; set; }

        public string? CargoTrackingNumber { get; set; }
        public DateTime? CargoDate { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;
        public decimal TotalPrice { get; set; }

        public string FullName { get; set; }

        public ICollection<EShopMVC.Models.Refund> Refunds { get; set; }

        public string Phone { get; set; }

        public string? IpAddress { get; set; }

        public DateTime? PaidAt { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public bool CancelRequested { get; set; } = false;

        public string Address { get; set; }

        // ✅ Admin onayladı mı?
        public bool CancelApproved { get; set; }

        // ❌ Admin reddetti mi?
        public bool CancelRejected { get; set; }

        public DateTime? CancelRequestedAt { get; set; }

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<PartialRefund> PartialRefunds { get; set; }
        public ICollection<FraudFlag> FraudFlags { get; set; } = new List<FraudFlag>();

        public ICollection<OrderLog> OrderLogs { get; set; } = new List<OrderLog>();
        public ICollection<PaymentLog> PaymentLogs { get; set; } = new List<PaymentLog>();

        public string? PaymentTransactionId { get; set; }
        public bool IsPaid { get; set; }

        public bool HasActiveHighFraud =>
            FraudFlags != null &&
            FraudFlags.Any(f => !f.IsResolved && f.Severity == FraudSeverity.High);

        public FraudSeverity? MaxFraudSeverity =>
        FraudFlags != null && FraudFlags.Any()
        ? FraudFlags.Max(f => f.Severity)
        : null;

        public int GetRuleHitCount(string ruleCode)
        {
            return FraudFlags.Count(f => f.RuleCode == ruleCode);
        }

        public bool RefundOverrideEnabled { get; set; } = false;
        public string? RefundOverrideNote { get; set; }
        public DateTime? RefundOverrideAt { get; set; }
        public string? RefundOverrideByUserId { get; set; }

        public bool CanRefund =>
    !HasActiveHighFraud || RefundOverrideEnabled;

        public bool HasActiveFraud =>
    FraudFlags != null && FraudFlags.Any(f => !f.IsResolved);

        public bool HasHighFraud =>
            FraudFlags != null && FraudFlags.Any(f =>
                !f.IsResolved && f.Severity == FraudSeverity.High);

        public Order()
        {
            Address = "CTOR DEFAULT";
        }

        [NotMapped]
        public List<TimelineItemVM> Timeline { get; set; } = new();

        public void MarkAsPaid()
        {
            Status = OrderStatus.Paid;

            AddDomainEvent(new OrderPaidEvent(Id));
        }
    }
}