using MediatR;
using SkillSwapAPI.Domain.Common.Results;

namespace SkillSwapAPI.Application.Features.Users.Commands.RemoveUserSkill;

public sealed record RemoveUserSkillCommand(Guid UserId, Guid UserSkillId) : IRequest<Result<Deleted>>;
