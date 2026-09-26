namespace SkillSwapAPI.Application.Features.Users.Dtos;

public sealed record ProfileDto(
    Guid UserId,
    string FirstName,
    string LastName,
    string Email,
    decimal AverageRating,
    int TotalReviewsCount,
    bool IsActive,
    DateTimeOffset CreatedAtUtc,
    IReadOnlyList<UserSkillDto> UserSkills);
