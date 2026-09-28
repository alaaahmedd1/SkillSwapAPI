using FluentValidation;

namespace SkillSwapAPI.Application.Features.Admin.Commands.UpdateUserStatus;

public sealed class UpdateUserStatusCommandValidator : AbstractValidator<UpdateUserStatusCommand>
{
    public UpdateUserStatusCommandValidator()
    {
        RuleFor(command => command.AdminId).NotEmpty();
        RuleFor(command => command.UserId).NotEmpty();
    }
}
