using MediatR;
using Microsoft.Extensions.Logging;
using SkillSwapAPI.Application.Common.Interfaces;
using SkillSwapAPI.Application.Common.Interfaces.Notifications;
using SkillSwapAPI.Domain.Common.Results;

namespace SkillSwapAPI.Application.Features.Identity.Commands.ResetPassword;

public sealed class ResetPasswordCommandHandler(
    IOtpService otpService,
    ILogger<ResetPasswordCommandHandler> logger)
    : IRequestHandler<ResetPasswordCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(ResetPasswordCommand command, CancellationToken ct)
    {
        logger.LogInformation("Resetting password for {Email}", command.Email);
        return await otpService.VerifyAndResetPasswordAsync(
            command.Email, command.Otp, command.NewPassword, ct);
    }
}