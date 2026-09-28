using MediatR;
using SkillSwapAPI.Application.Common.Interfaces.UnitOfWork;
using SkillSwapAPI.Application.Features.Skills.Dtos;
using SkillSwapAPI.Domain.Common.Results;
using SkillSwapAPI.Domain.Modules.Administration.Entities;
using SkillSwapAPI.Domain.Skills.Entities;

namespace SkillSwapAPI.Application.Features.Admin.Commands.CreateCategory;

public sealed class CreateCategoryCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<CreateCategoryCommand, Result<SkillCategoryDto>>
{
    public async Task<Result<SkillCategoryDto>> Handle(CreateCategoryCommand command, CancellationToken ct)
    {
        var category = new SkillCategory
        {
            Name = command.Name,
            Description = command.Description,
            IsActive = true
        };

        await unitOfWork.SkillCategories.AddAsync(category, ct);
        await unitOfWork.CompleteAsync(ct);

        await unitOfWork.AuditLogs.AddAsync(new AuditLog
        {
            Id = Guid.NewGuid(),
            AdminId = command.AdminId,
            Action = "CategoryCreated",
            TargetEntity = "SkillCategory",
            TargetEntityId = category.Id.ToString(),
            PerformedAtUtc = DateTimeOffset.UtcNow
        }, ct);

        await unitOfWork.CompleteAsync(ct);

        return new SkillCategoryDto(category.Id, category.Name, category.Description, []);
    }
}
