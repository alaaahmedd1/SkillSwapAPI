using FluentValidation;

namespace SkillSwapAPI.Application.Features.SessionProposals.Commands.AcceptProposal;

public sealed class AcceptProposalCommandValidator : AbstractValidator<AcceptProposalCommand>
{
    public AcceptProposalCommandValidator()
    {
        RuleFor(command => command.SwapRequestId).NotEmpty();
        RuleFor(command => command.ProposalId).NotEmpty();
        RuleFor(command => command.UserId).NotEmpty();
    }
}
