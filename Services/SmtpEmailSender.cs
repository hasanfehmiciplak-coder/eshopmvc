using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;

public class SmtpEmailSender : IEmailSender
{
    private readonly IConfiguration _config;

    public SmtpEmailSender(IConfiguration config)
    {
        _config = config;
    }

    // 🔹 Identity'nin beklediği
    public async Task SendAsync(string email, string subject, string htmlMessage)
    {
        await SendInternalAsync(email, subject, htmlMessage);
    }

    // 🔹 Projede HALA bir yerin beklediği
    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        await SendInternalAsync(email, subject, htmlMessage);
    }

    // 🔹 Gerçek SMTP logic TEK YERDE
    private async Task SendInternalAsync(string email, string subject, string htmlMessage)
    {
        var smtp = new SmtpClient
        {
            Host = _config["EmailSettings:SmtpServer"],
            Port = int.Parse(_config["EmailSettings:Port"]),
            EnableSsl = true,
            Credentials = new NetworkCredential(
                _config["EmailSettings:Username"],
                _config["EmailSettings:Password"]
            )
        };

        var mail = new MailMessage
        {
            From = new MailAddress(
                _config["EmailSettings:SenderEmail"],
                _config["EmailSettings:SenderName"]
            ),
            Subject = subject,
            Body = htmlMessage,
            IsBodyHtml = true
        };

        mail.To.Add(email);
        await smtp.SendMailAsync(mail);
    }
}