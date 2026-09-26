using System.Security.Claims;

namespace SkillSwapAPI.Application.Features.Identity.Dtos;

public sealed record AppUserDto(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    decimal AverageRating,
    int TotalReviewsCount,
    bool IsActive,
    DateTimeOffset CreatedAtUtc,
    IList<string> Roles,
    IList<Claim> Claims);
