using MediatR;
using SkillSwapAPI.Domain.Common.Results;
using SkillSwapAPI.Domain.Identity;

namespace SkillSwapAPI.Application.Features.Identity.Commands.SocialLogin;

public sealed record SocialLoginCommand(
    string IdToken,
    SocialProvider Provider
) : IRequest<Result<LoginResponse>>;
