using MediatR;
using SkillSwapAPI.Application.Common.Errors;
using SkillSwapAPI.Application.Common.Interfaces.UnitOfWork;
using SkillSwapAPI.Application.Features.Users.Dtos;
using SkillSwapAPI.Domain.Common.Results;
using SkillSwapAPI.Domain.Modules.Users.Entities;

namespace SkillSwapAPI.Application.Features.Users.Commands.AddUserSkill;

public sealed class AddUserSkillCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<AddUserSkillCommand, Result<UserSkillDto>>
{
    public async Task<Result<UserSkillDto>> Handle(AddUserSkillCommand command, CancellationToken ct)
    {
        var skill = await unitOfWork.Skills.GetWithCategoryAsync(command.SkillId, ct);
        if (skill is null)
        {
            return ApplicationErrors.Skills.SkillNotFound;
        }

        var hasOppositeType = await unitOfWork.UserSkills.HasSkillWithDifferentTypeAsync(
            command.UserId, command.SkillId, command.Type, ct);
        if (hasOppositeType)
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

        await unitOfWork.UserSkills.AddAsync(userSkill, ct);
        await unitOfWork.CompleteAsync(ct);

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
