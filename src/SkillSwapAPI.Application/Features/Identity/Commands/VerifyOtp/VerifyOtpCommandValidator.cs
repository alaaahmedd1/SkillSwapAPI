using FluentValidation;

namespace SkillSwapAPI.Application.Features.Identity.Commands.VerifyOtp;

public sealed class VerifyOtpCommandValidator : AbstractValidator<VerifyOtpCommand>
{
    public VerifyOtpCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Email is invalid.");
        RuleFor(x => x.Otp).NotEmpty().Matches(@"^\d{4,6}$").WithMessage("OTP must be 4-6 digits.");
    }
}
