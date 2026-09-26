namespace SkillSwapAPI.Application.Common.Interfaces.Notifications;


public interface IEmailService
{
    Task<bool> SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default);
}
