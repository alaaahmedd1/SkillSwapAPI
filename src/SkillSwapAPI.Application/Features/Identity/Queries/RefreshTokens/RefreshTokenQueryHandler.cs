using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SkillSwapAPI.Application.Common.Errors;
using SkillSwapAPI.Application.Common.Interfaces;
using SkillSwapAPI.Application.Common.Interfaces.Identity;
using SkillSwapAPI.Domain.Common.Results;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SkillSwapAPI.Application.Features.Identity.Queries.RefreshTokens;

public sealed class RefreshTokenQueryHandler(
    ITokenProvider tokenProvider,
    IIdentityService identityService,
    IApplicationDbContext context,
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

        var refreshToken = await context.RefreshTokens
            .FirstOrDefaultAsync(r => r.Token == query.RefreshToken && r.UserId == userId, ct);

        if (refreshToken is null || refreshToken.ExpiresOnUtc < DateTimeOffset.UtcNow)
        {
            logger.LogError("Refresh token expired or not found for user {UserId}", userId);
            return ApplicationErrors.Token.RefreshTokenExpired;
        }

        context.RefreshTokens.Remove(refreshToken);
        await context.SaveChangesAsync(ct);

        var userResult = await identityService.GetUserByIdAsync(userId, ct);
        if (userResult.IsError)
        {
            logger.LogError("Get user by id failed: {Error}", userResult.TopError.Description);
            return userResult.Errors;
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