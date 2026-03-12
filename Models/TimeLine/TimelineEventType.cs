namespace EShopMVC.Models.TimeLine
{
    public enum TimelineEventType
    {
        OrderCreated = 1,

        Refund = 10,
        UndoRefund = 11,

        PaymentReceived = 3,

        Fraud = 20,

        Debug = 99,
        FraudResolved = 100,
        Shipped = 2,
        Delivered = 4,
        Canceled = 5,
        Info = 50,
        Warning = 51,
        Error = 52,
        success = 53,
        PaymentFailed = 101,
        PaymentInitiated = 102,
        RefundRequested = 105,
        RefundCompleted = 106,

        FraudScoreCalculated = 107,
        FraudFlagCreated = 108,
        FraudDetected = 109
    }
}