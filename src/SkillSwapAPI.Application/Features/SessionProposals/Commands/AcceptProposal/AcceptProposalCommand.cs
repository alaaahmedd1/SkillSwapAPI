using MediatR;
using SkillSwapAPI.Domain.Common.Results;

namespace SkillSwapAPI.Application.Features.SessionProposals.Commands.AcceptProposal;

public sealed record AcceptProposalCommand(Guid SwapRequestId, Guid ProposalId, Guid UserId)
    : IRequest<Result<Updated>>;
