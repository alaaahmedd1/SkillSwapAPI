namespace SkillSwapAPI.Application.Features.Skills.Dtos;

public sealed record SkillCatalogItemDto(
    int Id,
    string Name,
    string? Description,
    IReadOnlyList<SkillDto> Skills);
