using MediatR;
using SkillSwapAPI.Application.Common.Interfaces;
using SkillSwapAPI.Application.Common.Interfaces.Notifications;
using SkillSwapAPI.Domain.Common.Results;

namespace SkillSwapAPI.Application.Features.Identity.Commands.ResendOtp;

public sealed class ResendOtpCommandHandler(IOtpService otpService)
    : IRequestHandler<ResendOtpCommand, Result<Success>>
{
    public async Task<Result<Success>> Handle(ResendOtpCommand command, CancellationToken ct)
    {
       var resulet= await otpService.SendEmailConfirmationAsync(command.Email, ct);
        if (resulet.IsError)
            return resulet.Errors;
        else
            return Result.Success;
    }
}