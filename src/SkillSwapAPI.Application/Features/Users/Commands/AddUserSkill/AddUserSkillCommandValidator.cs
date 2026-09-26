using FluentValidation;
using SkillSwapAPI.Domain.Modules.Users.Enums;

namespace SkillSwapAPI.Application.Features.Users.Commands.AddUserSkill;

public sealed class AddUserSkillCommandValidator : AbstractValidator<AddUserSkillCommand>
{
    public AddUserSkillCommandValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.SkillId).NotEmpty();
        RuleFor(command => command.Type).IsInEnum();
        RuleFor(command => command.ProficiencyLevel).IsInEnum();
        RuleFor(command => command.YearsOfExperience).GreaterThanOrEqualTo(0).When(command => command.YearsOfExperience.HasValue);
    }
}
