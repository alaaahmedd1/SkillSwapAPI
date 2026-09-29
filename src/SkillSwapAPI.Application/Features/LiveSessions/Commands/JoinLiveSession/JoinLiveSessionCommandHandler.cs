using MediatR;
using SkillSwapAPI.Application.Common.Errors;
using SkillSwapAPI.Application.Common.Interfaces.Services;
using SkillSwapAPI.Application.Common.Interfaces.UnitOfWork;
using SkillSwapAPI.Application.Features.LiveSessions.Dtos;
using SkillSwapAPI.Domain.Common.Results;
using SkillSwapAPI.Domain.Modules.LiveSessions.Entities;
using SkillSwapAPI.Domain.Modules.LiveSessions.Enums;
using SkillSwapAPI.Domain.Modules.SwapRequests.Enums;

namespace SkillSwapAPI.Application.Features.LiveSessions.Commands.JoinLiveSession;

public sealed class JoinLiveSessionCommandHandler(
    IUnitOfWork unitOfWork,
    ILiveSessionTokenProvider tokenProvider)
    : IRequestHandler<JoinLiveSessionCommand, Result<LiveSessionRoomDto>>
{
    public async Task<Result<LiveSessionRoomDto>> Handle(JoinLiveSessionCommand command, CancellationToken ct)
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

        var existingRoom = await unitOfWork.LiveSessionRooms.GetBySwapRequestIdAsync(command.SwapRequestId, ct);

        if (existingRoom is not null)
        {
            if (existingRoom.Status == LiveSessionStatus.Ended)
            {
                return ApplicationErrors.LiveSessions.RoomAlreadyEnded;
            }

            return ToDto(existingRoom);
        }

        var room = new LiveSessionRoom
        {
            Id = Guid.NewGuid(),
            SwapRequestId = swapRequest.Id,
            RoomToken = tokenProvider.GenerateRoomToken(),
            ScheduledStartTime = DateTimeOffset.UtcNow,
            Status = LiveSessionStatus.Waiting
        };

        await unitOfWork.LiveSessionRooms.AddAsync(room, ct);
        await unitOfWork.CompleteAsync(ct);

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
