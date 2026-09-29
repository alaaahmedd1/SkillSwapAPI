using MediatR;
using SkillSwapAPI.Application.Common.Errors;
using SkillSwapAPI.Application.Common.Interfaces.UnitOfWork;
using SkillSwapAPI.Application.Features.LiveSessions.Dtos;
using SkillSwapAPI.Domain.Common.Results;
using SkillSwapAPI.Domain.Modules.LiveSessions.Entities;
using SkillSwapAPI.Domain.Modules.LiveSessions.Enums;
using SkillSwapAPI.Domain.Modules.SwapRequests.Enums;

namespace SkillSwapAPI.Application.Features.LiveSessions.Commands.StartLiveSession;

public sealed class StartLiveSessionCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<StartLiveSessionCommand, Result<LiveSessionRoomDto>>
{
    public async Task<Result<LiveSessionRoomDto>> Handle(StartLiveSessionCommand command, CancellationToken ct)
    {
        var swapRequest = await unitOfWork.SwapRequests.FindAsync(
            item => item.Id == command.SwapRequestId, ct);

        if (swapRequest is null)
        {
            return ApplicationErrors.SwapRequests.NotFound;
        }

        if (swapRequest.Status != SwapRequestStatus.Accepted)
        {
            return ApplicationErrors.LiveSessions.SwapNotAccepted;
        }

        if (swapRequest.RequesterId != command.UserId && swapRequest.ReceiverId != command.UserId)
        {
            return ApplicationErrors.LiveSessions.NotParticipant;
        }

        var room = await unitOfWork.LiveSessionRooms.GetBySwapRequestIdAsync(command.SwapRequestId, ct);

        if (room is null)
        {
            return ApplicationErrors.LiveSessions.RoomNotFound;
        }

        if (room.Status == LiveSessionStatus.Ended)
        {
            return ApplicationErrors.LiveSessions.RoomAlreadyEnded;
        }

        if (room.Status == LiveSessionStatus.Waiting)
        {
            room.Status = LiveSessionStatus.InProgress;
            room.ActualStartTime = DateTimeOffset.UtcNow;

            unitOfWork.LiveSessionRooms.Update(room);
            await unitOfWork.CompleteAsync(ct);
        }

        return ToDto(room);
    }

    private static LiveSessionRoomDto ToDto(LiveSessionRoom room)
    {
        return new LiveSessionRoomDto(
            room.Id,
            room.SwapRequestId,
            room.RoomToken,
            room.ScheduledStartTime,
            room.ActualStartTime,
            room.ActualEndTime,
            room.DurationSeconds,
            room.Status);
    }
}
