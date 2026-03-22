using EShopMVC.Areas.Admin.ViewModels.Orders;
using EShopMVC.Models.Fraud;
using EShopMVC.Modules.Orders.Domain.Entities;
using EShopMVC.Modules.Orders.Domain.Logs;
using EShopMVC.Modules.Orders.Domain.Refunds;
using Refund = EShopMVC.Modules.Orders.Domain.Entities.Refund;

public class OrderDetailViewModel
{
    public Order Order { get; set; } = null!;

    public List<FraudFlagItemVM> FraudFlags { get; set; } = new();

    public List<Refund> Refunds { get; set; } = new(); // ✔ TEK KALDI

    public List<TimelineItemVM> Timeline { get; set; } = new();

    public Dictionary<string, int> RuleHitCounts { get; set; }

    public List<PaymentLog> PaymentLogs { get; set; } = new();

    public bool HasFraudFlag =>
        FraudFlags != null && FraudFlags.Any();

    public bool HasHighFraud =>
        FraudFlags != null &&
        FraudFlags.Any(f => f.Severity == FraudSeverity.High);

    public bool RefundOverrideEnabled =>
        Order?.RefundOverrideEnabled ?? false;

    public int PaymentAttemptCount { get; set; }
    public int SuccessfulPaymentCount { get; set; }
    public int FailedPaymentCount { get; set; }
}