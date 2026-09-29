namespace SkillSwapAPI.Application.Features.Badges.Dtos;

public sealed record BadgeDto(
    int Id,
    string Name,
    string Description,
    string IconUrl);
