namespace SkillSwapAPI.Application.Features.Users.Dtos;

public sealed record ProfileIdentityDto(
    Guid UserId,
    string FirstName,
    string LastName,
    string Email,
    decimal AverageRating,
    int TotalReviewsCount,
    bool IsActive,
    DateTimeOffset CreatedAtUtc,
    string? Title = null,
    string? Bio = null,
    string? City = null,
    string? Country = null,
    string? TimeZone = null,
    bool OpenForInstantSwaps = true,
    bool OnlineOnly = false,
    bool AutoMatchBarterRequests = false);
