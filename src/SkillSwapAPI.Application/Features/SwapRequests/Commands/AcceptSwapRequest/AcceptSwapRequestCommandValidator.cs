using FluentValidation;

namespace SkillSwapAPI.Application.Features.SwapRequests.Commands.AcceptSwapRequest;

public sealed class AcceptSwapRequestCommandValidator : AbstractValidator<AcceptSwapRequestCommand>
{
    public AcceptSwapRequestCommandValidator()
    {
        RuleFor(command => command.SwapRequestId).NotEmpty();
        RuleFor(command => command.UserId).NotEmpty();
    }
}
