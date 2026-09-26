namespace SkillSwapAPI.Application.Common.Models;

public sealed record SocialUserInfo(
    string ProviderUserId,
    string Email,
    string FullName,
    string Provider);
