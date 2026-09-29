using MediatR;
using SkillSwapAPI.Domain.Common.Results;

namespace SkillSwapAPI.Application.Features.SessionProposals.Commands.RejectProposal;

public sealed record RejectProposalCommand(Guid SwapRequestId, Guid ProposalId, Guid UserId)
    : IRequest<Result<Updated>>;
