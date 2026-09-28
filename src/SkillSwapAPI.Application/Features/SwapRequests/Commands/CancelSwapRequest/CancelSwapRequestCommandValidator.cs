using FluentValidation;

namespace SkillSwapAPI.Application.Features.SwapRequests.Commands.CancelSwapRequest;

public sealed class CancelSwapRequestCommandValidator : AbstractValidator<CancelSwapRequestCommand>
{
    public CancelSwapRequestCommandValidator()
    {
        RuleFor(command => command.SwapRequestId).NotEmpty();
        RuleFor(command => command.UserId).NotEmpty();
    }
}
