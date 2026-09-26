using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SkillSwapAPI.Application.Common.Interfaces;
using SkillSwapAPI.Application.Common.Security;
using SkillSwapAPI.Domain.Common.Results;

namespace SkillSwapAPI.Application.Features.Identity.Commands.Logout;

public sealed class LogoutCommandHandler(
    IApplicationDbContext context,
    ILogger<LogoutCommandHandler> logger)
    : IRequestHandler<LogoutCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(LogoutCommand command, CancellationToken ct)
    {
        var refreshTokenHash = RefreshTokenHasher.Hash(command.RefreshToken);
        var revokedCount = await context.RefreshTokens
            .Where(rt => rt.UserId == command.UserId && rt.Token == refreshTokenHash && !rt.IsRevoked)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(token => token.IsRevoked, true),
                ct);

        if (revokedCount > 0)
        {
            logger.LogInformation("Refresh token revoked for user {UserId}", command.UserId);
        }

        return Result.Success;
    }
}
