using MediatR;
using SkillSwapAPI.Application.Common.Interfaces;
using SkillSwapAPI.Application.Common.Interfaces.Notifications;
using SkillSwapAPI.Domain.Common.Results;

namespace SkillSwapAPI.Application.Features.Identity.Commands.VerifyOtp;

public sealed class VerifyOtpCommandHandler(IOtpService otpService)
    : IRequestHandler<VerifyOtpCommand, Result<Success>>
{
    public Task<Result<Success>> Handle(VerifyOtpCommand command, CancellationToken ct) =>
        otpService.VerifyEmailConfirmationAsync(command.Email, command.Otp, ct);
}
