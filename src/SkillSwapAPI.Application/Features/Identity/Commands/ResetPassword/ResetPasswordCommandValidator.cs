using FluentValidation;

namespace SkillSwapAPI.Application.Features.Identity.Commands.ResetPassword;

public sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Otp).NotEmpty().Matches(@"^\d{4,6}$");
        RuleFor(x => x.NewPassword)
            .NotEmpty().MinimumLength(8).WithMessage("Minimum 8 characters required.")
            .Matches(@"[A-Z]").WithMessage("Must contain uppercase.")
            .Matches(@"[0-9]").WithMessage("Must contain a digit.");
        RuleFor(x => x.ConfirmPassword)
            .NotEmpty()
            .Equal(x => x.NewPassword).WithMessage("Passwords do not match.");
    }
}
