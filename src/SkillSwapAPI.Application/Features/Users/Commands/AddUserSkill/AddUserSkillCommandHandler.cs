using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwapAPI.Application.Common.Errors;
using SkillSwapAPI.Application.Common.Interfaces;
using SkillSwapAPI.Application.Features.Users.Dtos;
using SkillSwapAPI.Domain.Common.Results;
using SkillSwapAPI.Domain.Modules.Users.Entities;

namespace SkillSwapAPI.Application.Features.Users.Commands.AddUserSkill;

public sealed class AddUserSkillCommandHandler(IApplicationDbContext context)
    : IRequestHandler<AddUserSkillCommand, Result<UserSkillDto>>
{
    public async Task<Result<UserSkillDto>> Handle(AddUserSkillCommand command, CancellationToken ct)
    {
        var skill = await context.Skills
            .AsNoTracking()
            .Include(item => item.Category)
            .SingleOrDefaultAsync(item => item.Id == command.SkillId, ct);
        if (skill is null)
        {
            return ApplicationErrors.Skills.SkillNotFound;
        }

        var oppositeTypeExists = await context.UserSkills.AnyAsync(
            item => item.UserId == command.UserId
                && item.SkillId == command.SkillId
                && item.Type != command.Type,
            ct);
        if (oppositeTypeExists)
        {
            return ApplicationErrors.Skills.DuplicateSkillType;
        }

        var userSkill = new UserSkill
        {
            Id = Guid.NewGuid(),
            UserId = command.UserId,
            SkillId = command.SkillId,
            Type = command.Type,
            ProficiencyLevel = command.ProficiencyLevel,
            YearsOfExperience = command.YearsOfExperience
        };

        context.UserSkills.Add(userSkill);
        await context.SaveChangesAsync(ct);

        return new UserSkillDto(
            userSkill.Id,
            skill.Id,
            skill.Name,
            skill.Category.Name,
            userSkill.Type,
            userSkill.ProficiencyLevel,
            userSkill.YearsOfExperience);
    }
}
