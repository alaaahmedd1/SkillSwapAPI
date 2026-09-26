namespace SkillSwapAPI.Infrastructure.Settings;



public sealed class GmailSettings
{
    public const string SectionName = "GmailSettings";
    public string SmtpServer { get; init; } = string.Empty;
    public int SmtpPort { get; init; }
    public string SenderEmail { get; init; } = string.Empty;
    public string SenderPassword { get; init; } = string.Empty;
    public string SenderName { get; init; } = string.Empty;
}
