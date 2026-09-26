using FluentValidation;

namespace SkillSwapAPI.Application.Features.Identity.Commands.ResendOtp;

public sealed class ResendOtpCommandValidator : AbstractValidator<ResendOtpCommand>
{
    public ResendOtpCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("Email is invalid.");
    }
}
