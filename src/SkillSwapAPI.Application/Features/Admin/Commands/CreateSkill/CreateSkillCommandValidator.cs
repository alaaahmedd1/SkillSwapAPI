using FluentValidation;
using SkillSwapAPI.Application.Common.Interfaces.UnitOfWork;

namespace SkillSwapAPI.Application.Features.Admin.Commands.CreateSkill;

public sealed class CreateSkillCommandValidator : AbstractValidator<CreateSkillCommand>
{
    public CreateSkillCommandValidator(IUnitOfWork unitOfWork)
    {
        RuleFor(command => command.AdminId).NotEmpty();
        RuleFor(command => command.CategoryId).GreaterThan(0);
        RuleFor(command => command.Name)
            .NotEmpty()
            .MaximumLength(100);
        RuleFor(command => command.Description).MaximumLength(500);

        RuleFor(command => command.CategoryId)
            .MustAsync(async (categoryId, ct) => await unitOfWork.SkillCategories.ExistsAsync(categoryId, ct))
            .WithMessage("The specified skill category does not exist.")
            .When(command => command.CategoryId > 0);

        RuleFor(command => command.Name)
            .MustAsync(async (name, ct) => !await unitOfWork.Skills.ExistsByNameAsync(name, ct))
            .WithMessage("A skill with this name already exists.")
            .When(command => !string.IsNullOrWhiteSpace(command.Name));
    }
}
