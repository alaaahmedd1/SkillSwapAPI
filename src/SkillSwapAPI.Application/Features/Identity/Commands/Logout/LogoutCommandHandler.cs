using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SkillSwapAPI.Application.Common.Interfaces;
using SkillSwapAPI.Domain.Common.Results;

namespace SkillSwapAPI.Application.Features.Identity.Commands.Logout;

public sealed class LogoutCommandHandler(
    IApplicationDbContext context,
    ILogger<LogoutCommandHandler> logger)
    : IRequestHandler<LogoutCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(LogoutCommand command, CancellationToken ct)
    {
        var tokens = await context.RefreshTokens
            .Where(rt => rt.UserId == command.UserId && rt.Token == command.RefreshToken)
            .ToListAsync(ct);

        if (tokens.Count > 0)
        {
            context.RefreshTokens.RemoveRange(tokens);
            await context.SaveChangesAsync(ct);
            logger.LogInformation("Refresh token revoked for user {UserId}", command.UserId);
        }

        return Result.Success;
    }
}
