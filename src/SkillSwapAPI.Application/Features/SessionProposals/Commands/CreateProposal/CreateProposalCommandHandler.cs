using MediatR;
using SkillSwapAPI.Application.Common.Errors;
using SkillSwapAPI.Application.Common.Interfaces.UnitOfWork;
using SkillSwapAPI.Application.Features.SessionProposals.Dtos;
using SkillSwapAPI.Domain.Common.Results;
using SkillSwapAPI.Domain.Modules.SessionProposals.Entities;
using SkillSwapAPI.Domain.Modules.SessionProposals.Enums;
using SkillSwapAPI.Domain.Modules.SwapRequests.Enums;

namespace SkillSwapAPI.Application.Features.SessionProposals.Commands.CreateProposal;

public sealed class CreateProposalCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<CreateProposalCommand, Result<SessionProposalDto>>
{
    private static readonly int[] AllowedDurations = [30, 60, 120];

    public async Task<Result<SessionProposalDto>> Handle(CreateProposalCommand command, CancellationToken ct)
    {
        if (!AllowedDurations.Contains(command.DurationMinutes))
        {
            return ApplicationErrors.Scheduling.InvalidDuration;
        }

        if (command.StartTime.AddMinutes(command.DurationMinutes) != command.EndTime)
        {
            return ApplicationErrors.Scheduling.TimeMismatch;
        }

        var utcToday = DateOnly.FromDateTime(DateTime.UtcNow);
        if (command.ScheduledDate < utcToday)
        {
            return ApplicationErrors.Scheduling.DateInPast;
        }

        var swapRequest = await unitOfWork.SwapRequests.FindAsync(
            item => item.Id == command.SwapRequestId, ct);

        if (swapRequest is null)
        {
            return ApplicationErrors.Scheduling.SwapNotFound;
        }

        if (command.ProposerId != swapRequest.RequesterId && command.ProposerId != swapRequest.ReceiverId)
        {
            return ApplicationErrors.Scheduling.NotParticipant;
        }

        if (swapRequest.Status is not (SwapRequestStatus.Pending or SwapRequestStatus.Accepted))
        {
            return ApplicationErrors.Scheduling.InvalidSwapState;
        }

        var activeProposal = await unitOfWork.SessionProposals.FindAsync(
            proposal => proposal.SwapRequestId == command.SwapRequestId
                && proposal.Status == ProposalStatus.Proposed,
            ct);

        if (activeProposal is not null)
        {
            return ApplicationErrors.Scheduling.ActiveProposalExists;
        }

        var proposal = new SessionProposal
        {
            Id = Guid.NewGuid(),
            SwapRequestId = swapRequest.Id,
            ProposerId = command.ProposerId,
            ScheduledDate = command.ScheduledDate,
            StartTime = command.StartTime,
            EndTime = command.EndTime,
            DurationMinutes = command.DurationMinutes,
            Status = ProposalStatus.Proposed,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        await unitOfWork.SessionProposals.AddAsync(proposal, ct);
        await unitOfWork.CompleteAsync(ct);

        return new SessionProposalDto(
            proposal.Id,
            proposal.SwapRequestId,
            proposal.ProposerId,
            proposal.ScheduledDate,
            proposal.StartTime,
            proposal.EndTime,
            proposal.DurationMinutes,
            proposal.Status,
            proposal.CreatedAtUtc);
    }
}
