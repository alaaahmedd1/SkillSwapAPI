using MediatR;
using SkillSwapAPI.Application.Features.SessionProposals.Dtos;
using SkillSwapAPI.Domain.Common.Results;

namespace SkillSwapAPI.Application.Features.SessionProposals.Commands.CreateProposal;

public sealed record CreateProposalCommand(
    Guid SwapRequestId,
    Guid ProposerId,
    DateOnly ScheduledDate,
    TimeOnly StartTime,
    TimeOnly EndTime,
    int DurationMinutes) : IRequest<Result<SessionProposalDto>>;
