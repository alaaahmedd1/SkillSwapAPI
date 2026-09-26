using SkillSwapAPI.Domain.Modules.Users.Enums;

namespace SkillSwapAPI.Application.Features.Users.Dtos;

public sealed record UserSkillDto(
    Guid Id,
    Guid SkillId,
    string SkillName,
    string CategoryName,
    SkillType Type,
    ProficiencyLevel ProficiencyLevel,
    int? YearsOfExperience);
