using MediatR;
using Microsoft.Extensions.Logging;
using SkillSwapAPI.Application.Common.Interfaces.Identity;
using SkillSwapAPI.Application.Features.Identity.Dtos;
using SkillSwapAPI.Domain.Common.Results;

namespace SkillSwapAPI.Application.Features.Identity.Commands.Login;

public sealed class LoginCommandHandler(
    IIdentityService identityService,
    ITokenProvider tokenProvider,
    ILogger<LoginCommandHandler> logger)
    : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    public async Task<Result<LoginResponse>> Handle(LoginCommand command, CancellationToken ct)
    {
        var authResult = await identityService.AuthenticateAsync(command.Email, command.Password, ct);
        if (authResult.IsError)
        {
            logger.LogWarning("Authentication failed for email {Email}", command.Email);
            return authResult.Errors;
        }

        var tokenResult = await tokenProvider.GenerateJwtTokenAsync(authResult.Value, ct);
        if (tokenResult.IsError)
        {
            logger.LogError("Token generation failed for email {Email}", command.Email);
            return tokenResult.Errors;
        }

        return new LoginResponse(
            tokenResult.Value.AccessToken,
            tokenResult.Value.RefreshToken,
            tokenResult.Value.ExpiresOnUtc,
            new UserDto(authResult.Value.UserId, authResult.Value.Email, null)
        );
    }
}
