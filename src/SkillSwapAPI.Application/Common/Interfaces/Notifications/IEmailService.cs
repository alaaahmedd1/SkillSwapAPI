namespace SkillSwapAPI.Application.Common.Interfaces.Notifications;


public interface IEmailService
{
    Task<bool> SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default);

    Task<bool> SendWithAttachmentAsync(
      string to,
      string subject,
      string htmlBody,
      byte[] attachment,
      string attachmentFileName,
      string contentType,
      CancellationToken ct = default);
}
