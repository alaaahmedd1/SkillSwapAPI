namespace SkillSwapAPI.Infrastructure.Settings;



public sealed class GmailSettings
{
    public const string SectionName = "GmailSettings";
    public string SmtpServer { get; init; } = "smtp.gmail.com";
    public int SmtpPort { get; init; } = 587;
    public string SenderEmail { get; init; } = string.Empty;
    public string SenderPassword { get; init; } = string.Empty;
    public string SenderName { get; init; } = "SkillSwapAPI";
}
