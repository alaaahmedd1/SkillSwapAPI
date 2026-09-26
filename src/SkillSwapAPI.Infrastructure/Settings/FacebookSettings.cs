namespace SkillSwapAPI.Infrastructure.Settings;

public sealed class FacebookSettings
{
    public string BaseUrl { get; init; } = string.Empty;
    public string AppId { get; init; } = string.Empty;
    public string AppSecret { get; init; } = string.Empty;
}