using MediatR;
using Microsoft.Extensions.Logging;
using SkillSwapAPI.Application.Common.Errors;
using SkillSwapAPI.Application.Common.Interfaces.Identity;
using SkillSwapAPI.Application.Common.Interfaces.UnitOfWork;
using SkillSwapAPI.Application.Common.Security;
using SkillSwapAPI.Domain.Common.Results;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SkillSwapAPI.Application.Features.Identity.Queries.RefreshTokens;

public sealed class RefreshTokenQueryHandler(
    ITokenProvider tokenProvider,
    IIdentityService identityService,
    IUnitOfWork unitOfWork,
    ILogger<RefreshTokenQueryHandler> logger)
    : IRequestHandler<RefreshTokenQuery, Result<TokenResponse>>
{
    public async Task<Result<TokenResponse>> Handle(RefreshTokenQuery query, CancellationToken ct)
    {
        var principal = tokenProvider.GetPrincipalFromExpiredToken(query.ExpiredAccessToken);
        if (principal is null)
        {
            logger.LogError("Expired access token is not valid");
            return ApplicationErrors.Token.ExpiredAccessTokenInvalid;
        }

        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
          ?? principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
          ?? principal.FindFirst("nameid")?.Value;

        if (userId is null)
        {
            logger.LogError("Token does not contain a valid userId claim");
            return ApplicationErrors.Token.UserIdClaimInvalid;
        }

        var refreshTokenHash = RefreshTokenHasher.Hash(query.RefreshToken);
        var refreshToken = await unitOfWork.RefreshTokens.GetByUserAndTokenAsync(userId, refreshTokenHash, ct);

        if (refreshToken is null)
        {
            logger.LogError("Refresh token expired or not found for user {UserId}", userId);
            return ApplicationErrors.Token.RefreshTokenExpired;
        }

        if (refreshToken.IsRevoked)
        {
            var activeTokens = await unitOfWork.RefreshTokens.GetActiveByUserAsync(userId, ct);

            foreach (var activeToken in activeTokens)
            {
                activeToken.IsRevoked = true;
            }

            unitOfWork.RefreshTokens.UpdateRange(activeTokens);
            await unitOfWork.CompleteAsync(ct);

            logger.LogWarning("SECURITY: refresh token reuse detected for user {UserId}; all active refresh tokens revoked", userId);
            return ApplicationErrors.Token.RefreshTokenReused;
        }

        if (refreshToken.ExpiresOnUtc < DateTimeOffset.UtcNow)
        {
            logger.LogError("Refresh token expired for user {UserId}", userId);
            return ApplicationErrors.Token.RefreshTokenExpired;
        }

        refreshToken.IsRevoked = true;

        unitOfWork.RefreshTokens.Update(refreshToken);
        await unitOfWork.CompleteAsync(ct);

        var userResult = await identityService.GetUserByIdAsync(userId, ct);
        if (userResult.IsError)
        {
            logger.LogError("Get user by id failed: {Error}", userResult.TopError.Description);
            return userResult.Errors;
        }

        if (!userResult.Value.IsActive)
        {
            logger.LogWarning("Refresh token denied for suspended user {UserId}", userId);
            return ApplicationErrors.Auth.AccountSuspended;
        }

        var tokenResult = await tokenProvider.GenerateJwtTokenAsync(userResult.Value, ct);
        if (tokenResult.IsError)
        {
            logger.LogError("Token generation failed: {Error}", tokenResult.TopError.Description);
            return ApplicationErrors.Token.GenerationFailed;
        }

        return tokenResult.Value;
    }
}
