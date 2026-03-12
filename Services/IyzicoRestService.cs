using EShopMVC.Models.Dtos;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

public class IyzicoRestService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    public IyzicoRestService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;
    }

    public async Task<string> CreateCheckoutFormAsync(object body)
    {
        var json = JsonSerializer.Serialize(body);

        var apiKey = _config["Iyzico:ApiKey"];
        var secretKey = _config["Iyzico:SecretKey"];
        var baseUrl = _config["Iyzico:BaseUrl"];

        var random = Guid.NewGuid().ToString();
        var hash = Convert.ToBase64String(
            SHA1.Create().ComputeHash(
                Encoding.UTF8.GetBytes(apiKey + random + secretKey)
            )
        );

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization",
            $"IYZWS {apiKey}:{hash}");
        _httpClient.DefaultRequestHeaders.Add("x-iyzi-rnd", random);
        _httpClient.DefaultRequestHeaders.Accept
            .Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await _httpClient.PostAsync(
            $"{baseUrl}/payment/checkoutform/initialize",
            new StringContent(json, Encoding.UTF8, "application/json")
        );

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }

    public async Task<string> RetrieveCheckoutFormAsync(string token)
    {
        var apiKey = _config["Iyzico:ApiKey"];
        var secretKey = _config["Iyzico:SecretKey"];
        var baseUrl = _config["Iyzico:BaseUrl"];

        var random = Guid.NewGuid().ToString();
        var hash = Convert.ToBase64String(
            SHA1.Create().ComputeHash(
                Encoding.UTF8.GetBytes(apiKey + random + secretKey)
            )
        );

        var body = JsonSerializer.Serialize(new { token });

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization",
            $"IYZWS {apiKey}:{hash}");
        _httpClient.DefaultRequestHeaders.Add("x-iyzi-rnd", random);
        _httpClient.DefaultRequestHeaders.Accept
            .Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await _httpClient.PostAsync(
            $"{baseUrl}/payment/checkoutform/auth/ecom/detail",
            new StringContent(body, Encoding.UTF8, "application/json")
        );

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }

    private async Task<string> SendRequestAsync(string endpoint, object body)
    {
        var json = JsonSerializer.Serialize(body);

        var apiKey = _config["Iyzico:ApiKey"];
        var secretKey = _config["Iyzico:SecretKey"];
        var baseUrl = _config["Iyzico:BaseUrl"];

        var random = Guid.NewGuid().ToString();
        var hash = Convert.ToBase64String(
            SHA1.Create().ComputeHash(
                Encoding.UTF8.GetBytes(apiKey + random + secretKey)
            )
        );

        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("Authorization",
            $"IYZWS {apiKey}:{hash}");
        _httpClient.DefaultRequestHeaders.Add("x-iyzi-rnd", random);
        _httpClient.DefaultRequestHeaders.Accept
            .Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var response = await _httpClient.PostAsync(
            $"{baseUrl}{endpoint}",
            new StringContent(json, Encoding.UTF8, "application/json")
        );

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }

    public async Task<IyzicoRefundResult> RefundAsync(
    string paymentTransactionId,
    decimal amount)
    {
        // iyzico RefundRequest burada
        // paymentTransactionId + amount gönderilir

        // response parse edilir
        return new IyzicoRefundResult
        {
            Status = "success",
            Raw = "{}"
        };
    }
}