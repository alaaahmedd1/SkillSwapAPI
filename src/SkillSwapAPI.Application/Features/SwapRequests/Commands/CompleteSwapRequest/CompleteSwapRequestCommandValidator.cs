using FluentValidation;

namespace SkillSwapAPI.Application.Features.SwapRequests.Commands.CompleteSwapRequest;

public sealed class CompleteSwapRequestCommandValidator : AbstractValidator<CompleteSwapRequestCommand>
{
    public CompleteSwapRequestCommandValidator()
    {
        RuleFor(command => command.SwapRequestId).NotEmpty();
        RuleFor(command => command.UserId).NotEmpty();
    }
}
