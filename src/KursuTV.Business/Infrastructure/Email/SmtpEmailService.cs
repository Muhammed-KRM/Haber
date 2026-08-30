using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using KursuTV.Business.Interfaces;

namespace KursuTV.Business.Infrastructure.Email;

public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _config;

    public SmtpEmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendEmailAsync(string to, string subject, string htmlBody)
    {
        var smtpSettings = _config.GetSection("SmtpSettings");

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(
            smtpSettings["SenderName"] ?? "KursuTV.com",
            smtpSettings["SenderEmail"] ?? "noreply@ozeders.com"));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder { HtmlBody = htmlBody };
        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        await client.ConnectAsync(
            smtpSettings["Host"] ?? "smtp-relay.brevo.com",
            int.Parse(smtpSettings["Port"] ?? "587"),
            SecureSocketOptions.StartTls);

        await client.AuthenticateAsync(
            smtpSettings["Username"] ?? "",
            smtpSettings["Password"] ?? "");

        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }

    public async Task SendTemplatedEmailAsync(string to, string templateName, Dictionary<string, string> replacements)
    {
        // Basit template sistemi: ÅŸablon adÄ±na gÃ¶re HTML oluÅŸtur
        string subject;
        string htmlBody;

        switch (templateName)
        {
            case "WelcomeEmail":
                subject = "KursuTV.com'a HoÅŸ Geldiniz! ğŸ“";
                htmlBody = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                        <h1 style='color: #6366f1;'>HoÅŸ Geldiniz, {replacements.GetValueOrDefault("FullName", "")}!</h1>
                        <p>KursuTV.com'a baÅŸarÄ±yla kayÄ±t oldunuz.</p>
                        <p>ArtÄ±k binlerce Ã¶ÄŸretmen ve Ã¶ÄŸrenci arasÄ±nda aradÄ±ÄŸÄ±nÄ±z dersi bulabilir veya ders verebilirsiniz.</p>
                        <a href='https://kursutv.com/panel' style='display:inline-block; padding:12px 24px; background:#6366f1; color:white; border-radius:8px; text-decoration:none;'>Panelime Git</a>
                    </div>";
                break;

            case "ListingApproved":
                subject = "Ä°lanÄ±nÄ±z OnaylandÄ±! âœ…";
                htmlBody = $@"
                    <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto;'>
                        <h1 style='color: #22c55e;'>Ä°lanÄ±nÄ±z YayÄ±nda!</h1>
                        <p><strong>{replacements.GetValueOrDefault("ListingTitle", "")}</strong> baÅŸlÄ±klÄ± ilanÄ±nÄ±z baÅŸarÄ±yla onaylandÄ± ve artÄ±k yayÄ±nda.</p>
                    </div>";
                break;

            default:
                subject = "KursuTV.com Bilgilendirme";
                htmlBody = "<p>Bu bir bilgilendirme mesajÄ±dÄ±r.</p>";
                break;
        }

        await SendEmailAsync(to, subject, htmlBody);
    }
}
