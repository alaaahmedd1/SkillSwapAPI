using MediatR;
using SkillSwapAPI.Application.Features.Users.Dtos;
using SkillSwapAPI.Domain.Common.Results;

namespace SkillSwapAPI.Application.Features.Admin.Commands.UpdateUserStatus;

public sealed record UpdateUserStatusCommand(
    Guid AdminId,
    Guid UserId,
    bool IsActive) : IRequest<Result<ProfileIdentityDto>>;
