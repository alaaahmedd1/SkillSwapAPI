using SkillSwapAPI.Domain.Modules.Users.Enums;

namespace SkillSwapAPI.Application.Features.Users.Dtos;

public sealed record UserSearchResultDto(
    Guid UserId,
    string FirstName,
    string LastName,
    decimal AverageRating,
    int TotalReviewsCount,
    IReadOnlyList<UserSkillSummaryDto> Skills);

public sealed record UserSkillSummaryDto(
    Guid SkillId,
    string SkillName,
    string CategoryName,
    SkillType Type,
    ProficiencyLevel ProficiencyLevel);
