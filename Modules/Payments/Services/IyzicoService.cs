using EShopMVC.Models.Dtos;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using EShopMVC.Modules.Payments.Public;

public class IyzicoService : IPaymentGateway
{
    private readonly IConfiguration _config;
    private readonly HttpClient _http;

    public IyzicoService(IConfiguration config)
    {
        _config = config;
        _http = new HttpClient();
    }

    public async Task<bool> ChargeAsync(int orderId, decimal amount)
    {
        // iyzico ödeme işlemi
        return true;
    }

    public async Task<IyzicoRefundResult> RefundAsync(
        string paymentTransactionId,
        decimal amount)
    {
        var apiKey = _config["Iyzico:ApiKey"];
        var secretKey = _config["Iyzico:SecretKey"];
        var baseUrl = _config["Iyzico:BaseUrl"];

        var requestBody = new
        {
            locale = "tr",
            conversationId = Guid.NewGuid().ToString(),
            paymentTransactionId = paymentTransactionId,
            price = amount.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture),
            currency = "TRY"
        };

        var json = JsonSerializer.Serialize(requestBody);

        // 🔐 iyzico AUTH HEADER
        var rnd = Guid.NewGuid().ToString();
        var hashStr = apiKey + rnd + secretKey + json;

        using var sha1 = SHA1.Create();
        var hash = Convert.ToBase64String(
            sha1.ComputeHash(Encoding.UTF8.GetBytes(hashStr))
        );

        var authHeader =
            $"IYZWS {apiKey}:{hash}";

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"{baseUrl}/payment/refund"
        );

        request.Headers.Add("Authorization", authHeader);
        request.Headers.Add("x-iyzi-rnd", rnd);
        request.Headers.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json")
        );

        request.Content = new StringContent(
            json,
            Encoding.UTF8,
            "application/json"
        );

        var response = await _http.SendAsync(request);
        var raw = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(raw);
        var status = doc.RootElement
            .GetProperty("status")
            .GetString();

        return new IyzicoRefundResult
        {
            Status = status,
            Raw = raw
        };
    }
}