using EShopMVC.Models.Fraud;
using OrderEntity = EShopMVC.Modules.Orders.Domain.Entities.Order;

namespace EShopMVC.Modules.Fraud.Models
{
    public class FraudFlag
    {
        public int Id { get; private set; }

        public int OrderId { get; private set; }

        public OrderEntity Order { get; private set; }
        public int? RefundId { get; set; }

        public FraudSeverity Severity { get; set; }

        public string RuleCode { get; set; }
        public string Description { get; set; }

        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

        public bool IsResolved { get; private set; } = false;
        public string ResolvedByUserId { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string ResolutionNote { get; set; }
        public bool IsActive { get; internal set; }

        public FraudReason Reason { get; private set; }

        public FraudCaseStatus CaseStatus { get; set; } = FraudCaseStatus.Open;

        // ✅ Constructor 1: FraudSeverity ile
        public FraudFlag(int orderId, string ruleCode, FraudSeverity severity, string description)
        {
            OrderId = orderId;
            RuleCode = ruleCode;
            Severity = severity;
            Description = description;

            CreatedAt = DateTime.UtcNow;
            IsResolved = false;
            IsActive = true;
        }

        // ✅ Constructor 2: FraudReason ile
        public FraudFlag(
            int orderId,
            string ruleCode,
            FraudSeverity severity,
            string description,
            int? refundId = null,
            FraudReason? reason = null)
        {
            OrderId = orderId;
            RuleCode = ruleCode;
            Severity = severity;
            Description = description;
            RefundId = refundId;
            Reason = reason ?? FraudReason.Unknown;

            CreatedAt = DateTime.UtcNow;
            IsResolved = false;
            IsActive = true;
        }

        public FraudFlag(int orderId, string ruleCode, FraudReason reason)
        {
            OrderId = orderId;
            RuleCode = ruleCode;
            Reason = reason;

            Severity = reason switch
            {
                FraudReason.RefundTooFast => FraudSeverity.Medium,
                FraudReason.RefundRetryExceeded => FraudSeverity.High,
                FraudReason.HighAmount => FraudSeverity.High,
                _ => FraudSeverity.Low
            };

            Description = reason switch
            {
                FraudReason.RefundTooFast => "Ödeme sonrası çok kısa sürede iade alındı",
                FraudReason.RefundRetryExceeded => "Refund maksimum retry sayısını aştı",
                FraudReason.HighAmount => "Sipariş tutarı anormal yüksek",
                _ => "Unknown"
            };

            CreatedAt = DateTime.UtcNow;
            IsResolved = false;
            IsActive = true;
        }

        public FraudFlag(int orderId, string ruleCode, FraudSeverity severity, string description, int? refundId, FraudReason reason)
        {
            OrderId = orderId;
            RuleCode = ruleCode;
            Severity = severity;
            Description = description;
            RefundId = refundId;
            Reason = reason;

            CreatedAt = DateTime.UtcNow;
            IsResolved = false;
            IsActive = true;
        }

        public static FraudFlag CreateRefundRetryExceeded(int orderId, int refundId)
        {
            return new FraudFlag(
                orderId,
                "REFUND_RETRY_EXCEEDED",
                FraudSeverity.High,
                "Refund maksimum retry sayısını aştı.",
                refundId,
                FraudReason.RefundRetryExceeded
            );
        }

        public void Resolve(string note, string userId)
        {
            IsResolved = true;
            ResolvedAt = DateTime.UtcNow;
            ResolutionNote = note;
            ResolvedByUserId = userId;
        }

        public static FraudFlag CreateRefundTooFast(int orderId)
        {
            return new FraudFlag(
                orderId,
                "REFUND_TOO_FAST",
                FraudSeverity.High,
                "Refund çok hızlı gerçekleşti",
                null,
                FraudReason.RefundTooFast
            );
        }
    }
}