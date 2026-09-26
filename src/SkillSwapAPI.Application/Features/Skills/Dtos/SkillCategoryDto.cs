namespace SkillSwapAPI.Application.Features.Skills.Dtos;

public sealed record SkillCategoryDto(
    int Id,
    string Name,
    string? Description,
    IReadOnlyList<SkillDto> Skills);