using FluentValidation;

namespace SkillSwapAPI.Application.Features.Users.Commands.UpdateProfile;

public sealed class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.FirstName).NotEmpty().MaximumLength(50);
        RuleFor(command => command.LastName).NotEmpty().MaximumLength(50);
        RuleFor(command => command.Title).MaximumLength(80);
        RuleFor(command => command.Bio).MaximumLength(200);
        RuleFor(command => command.City).MaximumLength(80);
        RuleFor(command => command.Country).MaximumLength(80);
        RuleFor(command => command.TimeZone).MaximumLength(100);
    }
}
