namespace SkillSwapAPI.Application.Features.Users.Dtos;

public sealed record ProfileIdentityDto(
    Guid UserId,
    string FirstName,
    string LastName,
    string Email,
    decimal AverageRating,
    int TotalReviewsCount,
    bool IsActive,
    DateTimeOffset CreatedAtUtc);
