using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SkillSwapAPI.Application.Common.Interfaces;
using SkillSwapAPI.Application.Common.Interfaces.Notifications;
using SkillSwapAPI.Infrastructure.Settings;
using System.Net;
using System.Net.Mail;

namespace SkillSwapAPI.Infrastructure.Services;

public sealed class GmailEmailService(
    IOptions<GmailSettings> options,
    ILogger<GmailEmailService> logger)
    : IEmailService
{
    private readonly GmailSettings _settings = options.Value;

    public async Task<bool> SendAsync(
        string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        try
        {
            using var client = new SmtpClient(_settings.SmtpServer, _settings.SmtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(_settings.SenderEmail, _settings.SenderPassword)
            };

            using var mail = new MailMessage
            {
                From = new MailAddress(_settings.SenderEmail, _settings.SenderName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true,
            };

            mail.To.Add(to);
            await client.SendMailAsync(mail, ct);

            logger.LogInformation("Email sent to {To}", to);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email to {To}", to);
            return false;
        }
    }
}