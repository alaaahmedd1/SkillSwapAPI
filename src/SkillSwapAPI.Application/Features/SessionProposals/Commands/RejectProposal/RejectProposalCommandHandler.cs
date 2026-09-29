using MediatR;
using SkillSwapAPI.Application.Common.Errors;
using SkillSwapAPI.Application.Common.Interfaces.UnitOfWork;
using SkillSwapAPI.Domain.Common.Results;
using SkillSwapAPI.Domain.Modules.SessionProposals.Enums;

namespace SkillSwapAPI.Application.Features.SessionProposals.Commands.RejectProposal;

public sealed class RejectProposalCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<RejectProposalCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(RejectProposalCommand command, CancellationToken ct)
    {
        var swapRequest = await unitOfWork.SwapRequests.FindAsync(
            item => item.Id == command.SwapRequestId, ct);

        if (swapRequest is null)
        {
            return ApplicationErrors.Scheduling.SwapNotFound;
        }

        if (command.UserId != swapRequest.RequesterId && command.UserId != swapRequest.ReceiverId)
        {
            return ApplicationErrors.Scheduling.NotParticipant;
        }

        var proposal = await unitOfWork.SessionProposals.FindAsync(
            item => item.Id == command.ProposalId && item.SwapRequestId == command.SwapRequestId,
            ct);

        if (proposal is null)
        {
            return ApplicationErrors.Scheduling.ProposalNotFound;
        }

        if (proposal.ProposerId == command.UserId)
        {
            return ApplicationErrors.Scheduling.CannotRejectOwnProposal;
        }

        if (proposal.Status != ProposalStatus.Proposed)
        {
            return ApplicationErrors.Scheduling.ProposalNotPending;
        }

        proposal.Status = ProposalStatus.Rejected;
        unitOfWork.SessionProposals.Update(proposal);
        await unitOfWork.CompleteAsync(ct);

        return Result.Updated;
    }
}