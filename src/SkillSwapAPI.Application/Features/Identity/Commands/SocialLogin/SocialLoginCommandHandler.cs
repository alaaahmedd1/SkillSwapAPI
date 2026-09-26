using MediatR;
using Microsoft.Extensions.Logging;
using SkillSwapAPI.Application.Common.Errors;
using SkillSwapAPI.Application.Common.Interfaces;
using SkillSwapAPI.Application.Common.Interfaces.Identity;
using SkillSwapAPI.Application.Common.Interfaces.Notifications;
using SkillSwapAPI.Application.Features.Identity.Dtos;
using SkillSwapAPI.Domain.Common.Results;

namespace SkillSwapAPI.Application.Features.Identity.Commands.SocialLogin;

public sealed class SocialLoginCommandHandler(
    ISocialAuthService socialAuthService,
    IIdentityService identityService,
    ITokenProvider tokenProvider,
    IApplicationDbContext context,
    ILogger<SocialLoginCommandHandler> logger)
    : IRequestHandler<SocialLoginCommand, Result<LoginResponse>>
{
    public async Task<Result<LoginResponse>> Handle(SocialLoginCommand command, CancellationToken ct)
    {
        var verifyResult = await socialAuthService.VerifyTokenAsync(
            command.IdToken, command.Provider, ct);

        if (verifyResult.IsError)
        {
            logger.LogWarning("Social token verification failed for provider {Provider}", command.Provider);
            return verifyResult.Errors;
        }

        var appUserResult = await identityService.FindOrCreateSocialUserAsync(
            verifyResult.Value, ct);

        if (appUserResult.IsError)
        {
            return appUserResult.Errors;
        }

        await context.SaveChangesAsync(ct);

        var tokenResult = await tokenProvider.GenerateJwtTokenAsync(appUserResult.Value, ct);
        if (tokenResult.IsError)
        {
            logger.LogError("Token generation failed after social login for {Email}", appUserResult.Value.Email);
            return ApplicationErrors.Token.GenerationFailed;
        }

        return new LoginResponse(
            tokenResult.Value.AccessToken,
            tokenResult.Value.RefreshToken,
            tokenResult.Value.ExpiresOnUtc,
            new UserDto(
                appUserResult.Value.UserId,
                appUserResult.Value.Email,
                appUserResult.Value.FirstName,
                appUserResult.Value.LastName,
                null)
        );
    }
}
