using Iyzipay;
using Iyzipay.Request;
using System.Globalization;

public class IyziRefundService
{
    private readonly Options _options;
    private readonly bool _enableRefund;

    public IyziRefundService(IConfiguration config)
    {
        _options = new Options
        {
            ApiKey = config["Iyzico:ApiKey"],
            SecretKey = config["Iyzico:SecretKey"],
            BaseUrl = config["Iyzico:BaseUrl"]
        };

        _enableRefund = config.GetValue<bool>("Iyzico:EnableRefund");
    }

    // 🔥 TEK VE NET METHOD
    public RefundResult CreateRefund(
        string paymentTransactionId,
        decimal amount)
    {
        // Sandbox guard
        if (!_enableRefund)
        {
            return new RefundResult
            {
                IsSuccess = true,
                ErrorMessage = "Sandbox ortamı – gerçek iade yapılmadı."
            };
        }

        var request = new CreateRefundRequest
        {
            PaymentTransactionId = paymentTransactionId,
            Price = amount.ToString("0.00", CultureInfo.InvariantCulture),
            Currency = Iyzipay.Model.Currency.TRY.ToString(),
            ConversationId = Guid.NewGuid().ToString()
        };

        // 🔥 SENKRON ÇAĞRI
        var refundResult =
            Iyzipay.Model.Refund.Create(request, _options);

        return new RefundResult
        {
            IsSuccess = refundResult.Status == "success",
            ErrorMessage = refundResult.ErrorMessage
        };
    }
}

public class RefundResult
{
    public bool IsSuccess { get; set; }
    public string ErrorMessage { get; set; }
}