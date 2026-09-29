using FluentValidation;

namespace SkillSwapAPI.Application.Features.SessionProposals.Commands.RejectProposal;

public sealed class RejectProposalCommandValidator : AbstractValidator<RejectProposalCommand>
{
    public RejectProposalCommandValidator()
    {
        RuleFor(command => command.SwapRequestId).NotEmpty();
        RuleFor(command => command.ProposalId).NotEmpty();
        RuleFor(command => command.UserId).NotEmpty();
    }
}
