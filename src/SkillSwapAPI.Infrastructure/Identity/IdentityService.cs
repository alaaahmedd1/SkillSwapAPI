using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SkillSwapAPI.Application.Common.Errors;
using SkillSwapAPI.Application.Common.Interfaces.Identity;
using SkillSwapAPI.Application.Common.Models;
using SkillSwapAPI.Application.Features.Identity.Dtos;
using SkillSwapAPI.Application.Features.Users.Dtos;
using SkillSwapAPI.Domain.Common.Results;
using System.Security.Claims;

namespace SkillSwapAPI.Infrastructure.Identity;

public sealed class IdentityService(
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager)
    : IIdentityService
{
    public async Task<Result<AppUserDto>> CreateAsync(
        string firstName, string lastName, string email, string password,
        CancellationToken ct = default)
    {
        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser is not null)
        {
            return ApplicationErrors.Auth.EmailAlreadyExists;
        }

        var user = new AppUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = false,
            FirstName = firstName,
            LastName = lastName,
            IsActive = true,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var firstError = result.Errors.FirstOrDefault();
            return Error.Validation(firstError?.Code ?? "User.CreateFailed", firstError?.Description ?? "Failed to create user.");
        }

        var roles = await userManager.GetRolesAsync(user);
        var claims = await userManager.GetClaimsAsync(user);

        return new AppUserDto(
            user.Id, user.Email!, user.FirstName, user.LastName,
            user.AverageRating, user.TotalReviewsCount, user.IsActive, user.CreatedAtUtc,
            roles, claims);
    }

    public async Task<Result<AppUserDto>> AuthenticateAsync(string email, string password, CancellationToken ct = default)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return ApplicationErrors.Auth.InvalidCredentials;
        }

        var result = await signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
            {
                return ApplicationErrors.Auth.AccountLockedOut;
            }

            return ApplicationErrors.Auth.InvalidCredentials;
        }

        if (!user.EmailConfirmed)
        {
            return ApplicationErrors.Auth.EmailNotVerified;
        }

        if (!user.IsActive)
        {
            return ApplicationErrors.Auth.AccountSuspended;
        }

        var roles = await userManager.GetRolesAsync(user);
        var claims = await userManager.GetClaimsAsync(user);

        return new AppUserDto(
            user.Id, user.Email!, user.FirstName, user.LastName,
            user.AverageRating, user.TotalReviewsCount, user.IsActive, user.CreatedAtUtc,
            roles, claims);
    }

    public async Task<Result<AppUserDto>> GetUserByIdAsync(string userId, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return ApplicationErrors.Auth.UserNotFound;
        }

        var roles = await userManager.GetRolesAsync(user);
        var claims = await userManager.GetClaimsAsync(user);

        return new AppUserDto(
            user.Id, user.Email!, user.FirstName, user.LastName,
            user.AverageRating, user.TotalReviewsCount, user.IsActive, user.CreatedAtUtc,
            roles, claims);
    }

    public async Task<bool> IsInRoleAsync(string userId, string role, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId);
        return user is not null && await userManager.IsInRoleAsync(user, role);
    }

    public async Task<string?> GetUserNameAsync(string userId, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId);
        return user?.UserName;
    }

    public async Task<Result<AppUserDto>> FindOrCreateSocialUserAsync(SocialUserInfo socialUser, CancellationToken ct = default)
    {
        var user = await userManager.FindByEmailAsync(socialUser.Email);
        if (user is null)
        {

            var (firstName, lastName) = SplitFullName(socialUser.FullName);

            user = new AppUser
            {
                UserName = socialUser.Email,
                Email = socialUser.Email,
                EmailConfirmed = true,
                FirstName = firstName,
                LastName = lastName,
                IsActive = true,
                CreatedAtUtc = DateTimeOffset.UtcNow
            };

            var createResult = await userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                var err = createResult.Errors.FirstOrDefault();
                return Error.Validation(err?.Code ?? "SocialUser.CreateFailed", err?.Description ?? "Failed to create social user.");
            }

            await userManager.AddClaimAsync(user, new Claim("LoginProvider", socialUser.Provider));
            await userManager.AddClaimAsync(user, new Claim("ProviderKey", socialUser.ProviderUserId));
        }

        var roles = await userManager.GetRolesAsync(user);
        var claims = await userManager.GetClaimsAsync(user);

        return new AppUserDto(
            user.Id, user.Email!, user.FirstName, user.LastName,
            user.AverageRating, user.TotalReviewsCount, user.IsActive, user.CreatedAtUtc,
            roles, claims);
    }

    public async Task<Result<Updated>> UpdateIdentityUserAsync(
        string identityId, string? email, string? currentPassword, string? newPassword,
        CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(identityId);
        if (user is null)
        {
            return ApplicationErrors.Auth.UserNotFound;
        }

        if (!string.IsNullOrWhiteSpace(email) && email != user.Email)
        {
            user.Email = email;
            user.UserName = email;
            await userManager.UpdateAsync(user);
        }

        if (!string.IsNullOrWhiteSpace(currentPassword) && !string.IsNullOrWhiteSpace(newPassword))
        {
            var passResult = await userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            if (!passResult.Succeeded)
            {
                var err = passResult.Errors.FirstOrDefault();
                return Error.Validation(err?.Code ?? "Password.ChangeFailed", err?.Description ?? "Failed to change password.");
            }
        }

        return Result.Updated;
    }

    public async Task<Result<ProfileIdentityDto>> GetProfileAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        return user is null ? ApplicationErrors.Auth.UserNotFound : ToProfileIdentityDto(user);
    }

    public async Task<IReadOnlyList<ProfileIdentityDto>> GetProfilesAsync(
        IReadOnlyCollection<Guid> userIds,
        CancellationToken ct = default)
    {
        if (userIds.Count == 0)
        {
            return [];
        }

        var users = new List<ProfileIdentityDto>();
        foreach (var userId in userIds)
        {
            ct.ThrowIfCancellationRequested();
            var user = await userManager.FindByIdAsync(userId.ToString());
            if (user is not null)
            {
                users.Add(ToProfileIdentityDto(user));
            }
        }

        return users;
    }

    public async Task<Result<Updated>> UpdateProfileAsync(
        Guid userId,
        string firstName,
        string lastName,
        CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return ApplicationErrors.Auth.UserNotFound;
        }

        user.FirstName = firstName;
        user.LastName = lastName;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var error = result.Errors.FirstOrDefault();
            return Error.Validation(error?.Code ?? "Profile.UpdateFailed", error?.Description ?? "Failed to update profile.");
        }

        return Result.Updated;
    }

    public async Task<Result<Updated>> UpdateProfileNamesAsync(
        string identityId,
        string firstName,
        string lastName,
        CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(identityId);
        if (user is null)
        {
            return ApplicationErrors.Auth.UserNotFound;
        }

        user.FirstName = firstName;
        user.LastName = lastName;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var error = result.Errors.FirstOrDefault();
            return Error.Validation(error?.Code ?? "Profile.UpdateFailed", error?.Description ?? "Failed to update profile.");
        }

        return Result.Updated;
    }

    public async Task<Result<Updated>> UpdateRatingSummaryAsync(
        Guid userId,
        decimal averageRating,
        int totalReviewsCount,
        CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return ApplicationErrors.Auth.UserNotFound;
        }

        user.AverageRating = averageRating;
        user.TotalReviewsCount = totalReviewsCount;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var error = result.Errors.FirstOrDefault();
            return Error.Validation(error?.Code ?? "Rating.UpdateFailed", error?.Description ?? "Failed to update rating summary.");
        }

        return Result.Updated;
    }

    public async Task<(IReadOnlyList<ProfileIdentityDto> Items, int TotalCount)> GetPagedUsersAsync(
        bool? isActive,
        string? searchTerm,
        int pageNumber,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = userManager.Users.AsNoTracking();

        if (isActive.HasValue)
        {
            query = query.Where(user => user.IsActive == isActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim();
            query = query.Where(user =>
                user.FirstName.Contains(term)
                || user.LastName.Contains(term)
                || (user.Email != null && user.Email.Contains(term)));
        }

        var totalCount = await query.CountAsync(ct);

        var users = await query
            .OrderByDescending(user => user.CreatedAtUtc)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var items = users.Select(ToProfileIdentityDto).ToList();

        return (items, totalCount);
    }

    public async Task<Result<ProfileIdentityDto>> UpdateUserStatusAsync(
        Guid userId,
        bool isActive,
        CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return ApplicationErrors.Auth.UserNotFound;
        }

        user.IsActive = isActive;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var error = result.Errors.FirstOrDefault();
            return Error.Validation(error?.Code ?? "Status.UpdateFailed", error?.Description ?? "Failed to update user status.");
        }

        return ToProfileIdentityDto(user);
    }

    private static (string FirstName, string LastName) SplitFullName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return (string.Empty, string.Empty);

        var parts = fullName.Trim().Split(' ', 2);
        return parts.Length == 2 ? (parts[0], parts[1]) : (parts[0], string.Empty);
    }

    private static ProfileIdentityDto ToProfileIdentityDto(AppUser user) => new(
        user.Id,
        user.FirstName,
        user.LastName,
        user.Email ?? string.Empty,
        user.AverageRating,
        user.TotalReviewsCount,
        user.IsActive,
        user.CreatedAtUtc);
}
