using MediatR;
using Microsoft.Extensions.Logging;
using SkillSwapAPI.Application.Common.Interfaces.Notifications;
using SkillSwapAPI.Domain.Common.Results;

namespace SkillSwapAPI.Application.Features.Identity.Commands.ForgotPassword;

public sealed class ForgotPasswordCommandHandler(
    IOtpService otpService,
    ILogger<ForgotPasswordCommandHandler> logger)
    : IRequestHandler<ForgotPasswordCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(ForgotPasswordCommand command, CancellationToken ct)
    {
        logger.LogInformation("Password reset OTP requested for {Email}", command.Email);
        await otpService.SendPasswordResetAsync(command.Email, ct);
        return Result.Success;
    }
}