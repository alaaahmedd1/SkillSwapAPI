using FluentValidation;
using SkillSwapAPI.Application.Common.Interfaces.UnitOfWork;

namespace SkillSwapAPI.Application.Features.Admin.Commands.CreateCategory;

public sealed class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator(IUnitOfWork unitOfWork)
    {
        RuleFor(command => command.AdminId).NotEmpty();
        RuleFor(command => command.Name)
            .NotEmpty()
            .MaximumLength(100);
        RuleFor(command => command.Description).MaximumLength(500);

        RuleFor(command => command.Name)
            .MustAsync(async (name, ct) => !await unitOfWork.SkillCategories.ExistsByNameAsync(name, ct))
            .WithMessage("A category with this name already exists.")
            .When(command => !string.IsNullOrWhiteSpace(command.Name));
    }
}
