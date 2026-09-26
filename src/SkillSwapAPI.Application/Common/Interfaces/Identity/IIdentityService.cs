using SkillSwapAPI.Application.Common.Models;
using SkillSwapAPI.Application.Features.Identity.Dtos;
using SkillSwapAPI.Domain.Common.Results;

namespace SkillSwapAPI.Application.Common.Interfaces.Identity;

public interface IIdentityService
{
    Task<Result<AppUserDto>> CreateAsync(string email, string password, CancellationToken ct = default);

    Task<Result<AppUserDto>> AuthenticateAsync(string email, string password, CancellationToken ct = default);

    Task<Result<AppUserDto>> GetUserByIdAsync(string userId, CancellationToken ct = default);

    Task<bool> IsInRoleAsync(string userId, string role, CancellationToken ct = default);

    Task<string?> GetUserNameAsync(string userId, CancellationToken ct = default);

    Task<Result<AppUserDto>> FindOrCreateSocialUserAsync(SocialUserInfo socialUser, CancellationToken ct = default);
    Task<Result<Updated>> UpdateIdentityUserAsync(
       string identityId,
       string? email,
       string? currentPassword,
       string? newPassword,
       CancellationToken ct = default);
}