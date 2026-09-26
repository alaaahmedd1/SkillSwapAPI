using MediatR;
using SkillSwapAPI.Domain.Common.Results;

namespace SkillSwapAPI.Application.Features.Identity.Commands.Logout;

public sealed record LogoutCommand(
    string RefreshToken,
    string UserId
) : IRequest<Result<Success>>;
