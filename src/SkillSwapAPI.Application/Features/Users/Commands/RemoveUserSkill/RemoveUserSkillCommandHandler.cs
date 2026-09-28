using MediatR;
using SkillSwapAPI.Application.Common.Errors;
using SkillSwapAPI.Application.Common.Interfaces.UnitOfWork;
using SkillSwapAPI.Domain.Common.Results;

namespace SkillSwapAPI.Application.Features.Users.Commands.RemoveUserSkill;

public sealed class RemoveUserSkillCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<RemoveUserSkillCommand, Result<Deleted>>
{
    public async Task<Result<Deleted>> Handle(RemoveUserSkillCommand command, CancellationToken ct)
    {
        var userSkill = await unitOfWork.UserSkills.GetByIdAndUserAsync(command.UserSkillId, command.UserId, ct);
        if (userSkill is null)
        {
            return ApplicationErrors.Skills.UserSkillNotFound;
        }

        unitOfWork.UserSkills.Delete(userSkill);
        await unitOfWork.CompleteAsync(ct);
        return Result.Deleted;
    }
}
