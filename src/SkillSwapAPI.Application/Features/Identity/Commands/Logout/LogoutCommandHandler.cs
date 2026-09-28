using MediatR;
using Microsoft.Extensions.Logging;
using SkillSwapAPI.Application.Common.Interfaces.UnitOfWork;
using SkillSwapAPI.Application.Common.Security;
using SkillSwapAPI.Domain.Common.Results;

namespace SkillSwapAPI.Application.Features.Identity.Commands.Logout;

public sealed class LogoutCommandHandler(
    IUnitOfWork unitOfWork,
    ILogger<LogoutCommandHandler> logger)
    : IRequestHandler<LogoutCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(LogoutCommand command, CancellationToken ct)
    {
        var refreshTokenHash = RefreshTokenHasher.Hash(command.RefreshToken);
        var refreshToken = await unitOfWork.RefreshTokens.GetActiveByUserAndTokenAsync(command.UserId, refreshTokenHash, ct);

        if (refreshToken is not null)
        {
            refreshToken.IsRevoked = true;

            unitOfWork.RefreshTokens.Update(refreshToken);
            await unitOfWork.CompleteAsync(ct);

            logger.LogInformation("Refresh token revoked for user {UserId}", command.UserId);
        }

        return Result.Success;
    }
}
