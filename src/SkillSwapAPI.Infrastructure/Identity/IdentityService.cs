using Microsoft.AspNetCore.Identity;
using SkillSwapAPI.Application.Common.Errors;
using SkillSwapAPI.Application.Common.Interfaces.Identity;
using SkillSwapAPI.Application.Common.Models;
using SkillSwapAPI.Application.Features.Identity.Dtos;
using SkillSwapAPI.Domain.Common.Results;
using System.Security.Claims;

namespace SkillSwapAPI.Infrastructure.Identity;

public sealed class IdentityService(
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager)
    : IIdentityService
{
    public async Task<Result<AppUserDto>> CreateAsync(string email, string password, CancellationToken ct = default)
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
            EmailConfirmed = false
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            var firstError = result.Errors.FirstOrDefault();
            return Error.Validation(firstError?.Code ?? "User.CreateFailed", firstError?.Description ?? "Failed to create user.");
        }

        var roles = await userManager.GetRolesAsync(user);
        var claims = await userManager.GetClaimsAsync(user);

        return new AppUserDto(user.Id, user.Email!, roles, claims);
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

        var roles = await userManager.GetRolesAsync(user);
        var claims = await userManager.GetClaimsAsync(user);

        return new AppUserDto(user.Id, user.Email!, roles, claims);
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

        return new AppUserDto(user.Id, user.Email!, roles, claims);
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
            user = new AppUser
            {
                UserName = socialUser.Email,
                Email = socialUser.Email,
                EmailConfirmed = true
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

        return new AppUserDto(user.Id, user.Email!, roles, claims);
    }

    public async Task<Result<Updated>> UpdateIdentityUserAsync(
        string identityId,
        string? email,
        string? currentPassword,
        string? newPassword,
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
}
