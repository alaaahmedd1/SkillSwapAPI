using MediatR;
using Microsoft.Extensions.Logging;
using SkillSwapAPI.Application.Common.Interfaces.Identity;
using SkillSwapAPI.Application.Common.Interfaces.Notifications;
using SkillSwapAPI.Application.Features.Identity.Dtos;
using SkillSwapAPI.Domain.Common.Results;

namespace SkillSwapAPI.Application.Features.Identity.Commands.Register;

public sealed class RegisterCommandHandler(
    IIdentityService identityService,
    IOtpService otpService,
    ILogger<RegisterCommandHandler> logger)
    : IRequestHandler<RegisterCommand, Result<UserDto>>
{
    public async Task<Result<UserDto>> Handle(RegisterCommand command, CancellationToken ct)
    {
        var result = await identityService.CreateAsync(
            command.FirstName, command.LastName, command.Email, command.Password, ct);

        if (result.IsError)
        {
            logger.LogWarning("Registration failed for email {Email}", command.Email);
            return result.Errors;
        }

        await otpService.SendEmailConfirmationAsync(command.Email, ct);

        return new UserDto(
            result.Value.UserId,
            result.Value.Email,
            result.Value.FirstName,
            result.Value.LastName,
            null
        );
    }
}