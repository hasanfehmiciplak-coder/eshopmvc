namespace EShopMVC.Models.Fraud
{
    public enum FraudReason
    {
        MultipleRefundsSameOrder = 1,
        FrequentUserRefunds = 2,
        HighRefundRatio = 3,
        RefundRetryExceeded = 4,
        RefundTooFast = 5,
        MultipleRefunds = 6,
        SuspiciousActivity = 7
    }
}