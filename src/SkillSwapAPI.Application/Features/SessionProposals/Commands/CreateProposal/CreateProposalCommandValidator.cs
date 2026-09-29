using FluentValidation;

namespace SkillSwapAPI.Application.Features.SessionProposals.Commands.CreateProposal;

public sealed class CreateProposalCommandValidator : AbstractValidator<CreateProposalCommand>
{
    private static readonly int[] AllowedDurations = [30, 60, 120];

    public CreateProposalCommandValidator()
    {
        RuleFor(command => command.SwapRequestId).NotEmpty();
        RuleFor(command => command.ProposerId).NotEmpty();
        RuleFor(command => command.ScheduledDate)
            .NotEqual(default(DateOnly))
            .WithMessage("Scheduled date is required.");
        RuleFor(command => command.StartTime)
            .LessThan(command => command.EndTime)
            .WithMessage("Start time must be before end time.");
        RuleFor(command => command.DurationMinutes)
            .Must(duration => AllowedDurations.Contains(duration))
            .WithMessage("Duration must be exactly 30, 60, or 120 minutes.");
    }
}
