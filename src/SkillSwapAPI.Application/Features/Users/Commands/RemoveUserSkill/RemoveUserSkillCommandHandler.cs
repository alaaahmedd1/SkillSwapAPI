using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwapAPI.Application.Common.Errors;
using SkillSwapAPI.Application.Common.Interfaces;
using SkillSwapAPI.Domain.Common.Results;

namespace SkillSwapAPI.Application.Features.Users.Commands.RemoveUserSkill;

public sealed class RemoveUserSkillCommandHandler(IApplicationDbContext context)
    : IRequestHandler<RemoveUserSkillCommand, Result<Deleted>>
{
    public async Task<Result<Deleted>> Handle(RemoveUserSkillCommand command, CancellationToken ct)
    {
        var userSkill = await context.UserSkills.SingleOrDefaultAsync(
            item => item.Id == command.UserSkillId && item.UserId == command.UserId,
            ct);
        if (userSkill is null)
        {
            return ApplicationErrors.Skills.UserSkillNotFound;
        }

        context.UserSkills.Remove(userSkill);
        await context.SaveChangesAsync(ct);
        return Result.Deleted;
    }
}
