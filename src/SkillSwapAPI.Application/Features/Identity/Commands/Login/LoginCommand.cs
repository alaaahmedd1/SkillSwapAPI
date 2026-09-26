using MediatR;
using SkillSwapAPI.Domain.Common.Results;

namespace SkillSwapAPI.Application.Features.Identity.Commands.Login;

public sealed record LoginCommand(
    string Email,
    string Password
) : IRequest<Result<LoginResponse>>;
