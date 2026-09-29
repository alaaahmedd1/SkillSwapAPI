using MediatR;
using SkillSwapAPI.Application.Common.Errors;
using SkillSwapAPI.Application.Common.Interfaces.Services;
using SkillSwapAPI.Application.Common.Interfaces.UnitOfWork;
using SkillSwapAPI.Domain.Common.Results;
using SkillSwapAPI.Domain.Modules.Chat.Entities;
using SkillSwapAPI.Domain.Modules.LiveSessions.Entities;
using SkillSwapAPI.Domain.Modules.LiveSessions.Enums;
using SkillSwapAPI.Domain.Modules.SessionProposals.Enums;
using SkillSwapAPI.Domain.Modules.SwapRequests.Enums;

namespace SkillSwapAPI.Application.Features.SessionProposals.Commands.AcceptProposal;

public sealed class AcceptProposalCommandHandler(
    IUnitOfWork unitOfWork,
    ILiveSessionTokenProvider tokenProvider)
    : IRequestHandler<AcceptProposalCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(AcceptProposalCommand command, CancellationToken ct)
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
            return ApplicationErrors.Scheduling.CannotAcceptOwnProposal;
        }

        if (proposal.Status != ProposalStatus.Proposed)
        {
            return ApplicationErrors.Scheduling.ProposalNotPending;
        }

        if (swapRequest.Status is not (SwapRequestStatus.Pending or SwapRequestStatus.Accepted))
        {
            return ApplicationErrors.Scheduling.InvalidSwapState;
        }

        proposal.Status = ProposalStatus.Accepted;
        unitOfWork.SessionProposals.Update(proposal);

        if (swapRequest.Status == SwapRequestStatus.Pending)
        {
            swapRequest.Status = SwapRequestStatus.Accepted;
            swapRequest.UpdatedAtUtc = DateTimeOffset.UtcNow;
            unitOfWork.SwapRequests.Update(swapRequest);

            var existingConversation = await unitOfWork.Conversations.FindAsync(
                conversation => conversation.SwapRequestId == swapRequest.Id, ct);

            if (existingConversation is null)
            {
                await unitOfWork.Conversations.AddAsync(new Conversation
                {
                    Id = Guid.NewGuid(),
                    SwapRequestId = swapRequest.Id,
                    CreatedAtUtc = DateTimeOffset.UtcNow
                }, ct);
            }
        }

        var scheduledStartTime = new DateTimeOffset(
            proposal.ScheduledDate.ToDateTime(proposal.StartTime, DateTimeKind.Utc));

        var room = await unitOfWork.LiveSessionRooms.GetBySwapRequestIdAsync(swapRequest.Id, ct);

        if (room is null)
        {
            await unitOfWork.LiveSessionRooms.AddAsync(new LiveSessionRoom
            {
                Id = Guid.NewGuid(),
                SwapRequestId = swapRequest.Id,
                RoomToken = tokenProvider.GenerateRoomToken(),
                ScheduledStartTime = scheduledStartTime,
                Status = LiveSessionStatus.Waiting
            }, ct);
        }
        else
        {
            room.ScheduledStartTime = scheduledStartTime;
            unitOfWork.LiveSessionRooms.Update(room);
        }

        await unitOfWork.CompleteAsync(ct);

        return Result.Updated;
    }
}
