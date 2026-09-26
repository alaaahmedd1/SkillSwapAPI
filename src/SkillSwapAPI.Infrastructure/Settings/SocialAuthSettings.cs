namespace SkillSwapAPI.Infrastructure.Settings;



public sealed class SocialAuthSettings
{
    public const string SectionName = "SocialAuth";
    public GoogleSettings Google { get; init; } = new();
    public FacebookSettings Facebook { get; init; } = new();
    public AppleSettings Apple { get; init; } = new();
}
