using FluentValidation;

namespace SkillSwapAPI.Application.Features.SwapRequests.Commands.RejectSwapRequest;

public sealed class RejectSwapRequestCommandValidator : AbstractValidator<RejectSwapRequestCommand>
{
    public RejectSwapRequestCommandValidator()
    {
        RuleFor(command => command.SwapRequestId).NotEmpty();
        RuleFor(command => command.UserId).NotEmpty();
    }
}
