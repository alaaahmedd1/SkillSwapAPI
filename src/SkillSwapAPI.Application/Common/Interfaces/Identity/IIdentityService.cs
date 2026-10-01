using SkillSwapAPI.Application.Common.Models;
using SkillSwapAPI.Application.Features.Identity.Dtos;
using SkillSwapAPI.Application.Features.Users.Dtos;
using SkillSwapAPI.Domain.Common.Results;

namespace SkillSwapAPI.Application.Common.Interfaces.Identity;

public interface IIdentityService
{
    Task<Result<AppUserDto>> CreateAsync(
        string firstName, string lastName, string email, string password,
        CancellationToken ct = default);

    Task<Result<AppUserDto>> AuthenticateAsync(
        string email, string password, CancellationToken ct = default);

    Task<Result<AppUserDto>> GetUserByIdAsync(string userId, CancellationToken ct = default);

    Task<bool> IsInRoleAsync(string userId, string role, CancellationToken ct = default);

    Task<string?> GetUserNameAsync(string userId, CancellationToken ct = default);

    Task<Result<AppUserDto>> FindOrCreateSocialUserAsync(
        SocialUserInfo socialUser, CancellationToken ct = default);

    Task<Result<Updated>> UpdateIdentityUserAsync(
        string identityId, string? email, string? currentPassword, string? newPassword,
        CancellationToken ct = default);

    Task<Result<ProfileIdentityDto>> GetProfileAsync(Guid userId, CancellationToken ct = default);

    Task<IReadOnlyList<ProfileIdentityDto>> GetProfilesAsync(
        IReadOnlyCollection<Guid> userIds,
        CancellationToken ct = default);

    Task<Result<Updated>> UpdateProfileAsync(
        Guid userId,
        string firstName,
        string lastName,
        CancellationToken ct = default);

    Task<Result<Updated>> UpdateProfileNamesAsync(
        string identityId,
        string firstName,
        string lastName,
        string? title = null,
        string? bio = null,
        string? city = null,
        string? country = null,
        string? timeZone = null,
        bool? openForInstantSwaps = null,
        bool? onlineOnly = null,
        bool? autoMatchBarterRequests = null,
        CancellationToken ct = default);

    Task<Result<Updated>> UpdateRatingSummaryAsync(
        Guid userId,
        decimal averageRating,
        int totalReviewsCount,
        CancellationToken ct = default);

    Task<(IReadOnlyList<ProfileIdentityDto> Items, int TotalCount)> GetPagedUsersAsync(
        bool? isActive,
        string? searchTerm,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default);

    Task<Result<ProfileIdentityDto>> UpdateUserStatusAsync(
        Guid userId,
        bool isActive,
        CancellationToken ct = default);
}
