using MediatR;
using SkillSwapAPI.Application.Common.Errors;
using SkillSwapAPI.Application.Common.Interfaces.UnitOfWork;
using SkillSwapAPI.Application.Features.Skills.Dtos;
using SkillSwapAPI.Domain.Common.Results;
using SkillSwapAPI.Domain.Modules.Administration.Entities;
using SkillSwapAPI.Domain.Skills.Entities;

namespace SkillSwapAPI.Application.Features.Admin.Commands.CreateSkill;

public sealed class CreateSkillCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<CreateSkillCommand, Result<SkillDto>>
{
    public async Task<Result<SkillDto>> Handle(CreateSkillCommand command, CancellationToken ct)
    {
        var categoryExists = await unitOfWork.SkillCategories.ExistsAsync(command.CategoryId, ct);

        if (!categoryExists)
        {
            return ApplicationErrors.Skills.SkillCategoryNotFound;
        }

        var skill = new Skill
        {
            Id = Guid.NewGuid(),
            CategoryId = command.CategoryId,
            Name = command.Name,
            Description = command.Description
        };

        await unitOfWork.Skills.AddAsync(skill, ct);

        await unitOfWork.AuditLogs.AddAsync(new AuditLog
        {
            Id = Guid.NewGuid(),
            AdminId = command.AdminId,
            Action = "SkillCreated",
            TargetEntity = "Skill",
            TargetEntityId = skill.Id.ToString(),
            PerformedAtUtc = DateTimeOffset.UtcNow
        }, ct);

        await unitOfWork.CompleteAsync(ct);

        return new SkillDto(skill.Id, skill.Name, skill.Description);
    }
}
