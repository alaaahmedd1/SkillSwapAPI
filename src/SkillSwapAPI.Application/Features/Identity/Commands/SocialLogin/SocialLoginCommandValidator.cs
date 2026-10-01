using FluentValidation;

namespace SkillSwapAPI.Application.Features.Identity.Commands.SocialLogin
{
    public sealed class SocialLoginCommandValidator : AbstractValidator<SocialLoginCommand>
    {
        public SocialLoginCommandValidator()
        {
            RuleFor(x => x.IdToken).NotEmpty().WithMessage("Social token is required.");
            RuleFor(x => x.Provider).IsInEnum().WithMessage("Provider must be Google, Facebook, or Apple.");
        }
    }
}
