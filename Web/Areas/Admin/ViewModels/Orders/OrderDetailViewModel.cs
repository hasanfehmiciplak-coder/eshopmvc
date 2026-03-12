using EShopMVC.Models;
using EShopMVC.Models.Fraud;
using EShopMVC.Modules.Orders.Models;
using EShopMVC.Modules.Payments.Models;

namespace EShopMVC.Areas.Admin.ViewModels.Orders
{
    public class OrderDetailViewModel
    {
        public Order Order { get; set; } = null!;

        public List<FraudFlagItemVM> FraudFlags { get; set; } = new();

        public List<TimelineItemVM> Timeline { get; set; } = new();

        public Dictionary<string, int> RuleHitCounts { get; set; }

        public List<RefundLog> RefundLogs { get; set; }
        public List<PaymentLog> PaymentLogs { get; set; }

        // 🔹 Genel fraud var mı
        public bool HasFraudFlag =>
            FraudFlags != null && FraudFlags.Any();

        // 🔥 HIGH fraud var mı
        public bool HasHighFraud =>
            FraudFlags != null &&
            FraudFlags.Any(f => f.Severity == FraudSeverity.High);

        // 🔥 Order’dan gelen override bilgisi
        public bool RefundOverrideEnabled =>
            Order?.RefundOverrideEnabled ?? false;

        public int PaymentAttemptCount { get; set; }
        public int SuccessfulPaymentCount { get; set; }
        public int FailedPaymentCount { get; set; }
    }
}