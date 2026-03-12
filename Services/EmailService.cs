using EShopMVC.Areas.Admin.ViewModels;
using EShopMVC.Models;
using EShopMVC.Services;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using EShopMVC.Options;

public class EmailService : IEmailService
{
    private readonly EShopMVC.Options.EmailSettings _settings;

    public EmailService(IOptions<EShopMVC.Options.EmailSettings> options)
    {
        _settings = options.Value;
    }

    public async Task SendOrderSuccessMailAsync(OrderSuccessMailVM model)
    {
        var html = LoadTemplate("OrderSuccess.html");

        html = html
            .Replace("{{OrderId}}", model.OrderId.ToString())
            .Replace("{{TotalPrice}}", model.TotalPrice.ToString("N2"))
            .Replace("{{OrderDate}}", model.OrderDate.ToString("dd.MM.yyyy HH:mm"));

        await SendAsync(
            model.UserEmail,
            "Siparişiniz Alındı 🎉",
            html
        );
    }

    private async Task SendAsync(string to, string subject, string htmlBody)
    {
        var message = new MailMessage
        {
            From = new MailAddress(
                _settings.FromEmail,
                _settings.FromName
            ),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };

        message.To.Add(to);

        using var smtp = new SmtpClient(
            _settings.SmtpHost,
            _settings.SmtpPort)
        {
            Credentials = new NetworkCredential(
                _settings.Username,
                _settings.Password
            ),
            EnableSsl = true
        };

        await smtp.SendMailAsync(message);
    }

    private string LoadTemplate(string templateName)
    {
        var path = Path.Combine(
            Directory.GetCurrentDirectory(),
            "EmailTemplates",
            templateName
        );

        return File.ReadAllText(path);
    }

    public async Task SendPaymentFailedMailAsync(PaymentFailedMailVM model)
    {
        var html = LoadTemplate("PaymentFailed.html");

        html = html
            .Replace("{{OrderId}}", model.OrderId.ToString())
            .Replace("{{ErrorMessage}}", model.ErrorMessage ?? "Bilinmeyen hata");

        await SendAsync(
            model.UserEmail,
            "Ödeme Başarısız ❌",
            html
        );
    }
}