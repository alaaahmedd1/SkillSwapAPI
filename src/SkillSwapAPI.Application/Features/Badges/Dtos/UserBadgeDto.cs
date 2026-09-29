namespace SkillSwapAPI.Application.Features.Badges.Dtos;

public sealed record UserBadgeDto(
    int BadgeId,
    string Name,
    string Description,
    string IconUrl,
    int AwardCount,
    DateTimeOffset LastAwardedAtUtc);
