using MediatR;
using SkillSwapAPI.Application.Common.Errors;
using SkillSwapAPI.Application.Common.Interfaces.UnitOfWork;
using SkillSwapAPI.Application.Features.LiveSessions.Dtos;
using SkillSwapAPI.Domain.Common.Results;
using SkillSwapAPI.Domain.Modules.LiveSessions.Entities;
using SkillSwapAPI.Domain.Modules.LiveSessions.Enums;
using SkillSwapAPI.Domain.Modules.SwapRequests.Enums;

namespace SkillSwapAPI.Application.Features.LiveSessions.Commands.SaveWhiteboardSnapshot;

public sealed class SaveWhiteboardSnapshotCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<SaveWhiteboardSnapshotCommand, Result<WhiteboardSnapshotDto>>
{
    public async Task<Result<WhiteboardSnapshotDto>> Handle(SaveWhiteboardSnapshotCommand command, CancellationToken ct)
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

        var now = DateTimeOffset.UtcNow;
        var snapshot = await unitOfWork.WhiteboardSnapshots.GetByRoomIdAsync(room.Id, ct);

        if (snapshot is null)
        {
            snapshot = new WhiteboardSnapshot
            {
                Id = Guid.NewGuid(),
                RoomId = room.Id,
                CanvasDataJson = command.CanvasDataJson,
                UpdatedAtUtc = now
            };

            await unitOfWork.WhiteboardSnapshots.AddAsync(snapshot, ct);
        }
        else
        {
            snapshot.CanvasDataJson = command.CanvasDataJson;
            snapshot.UpdatedAtUtc = now;

            unitOfWork.WhiteboardSnapshots.Update(snapshot);
        }

        await unitOfWork.CompleteAsync(ct);

        return new WhiteboardSnapshotDto(
            snapshot.Id,
            snapshot.RoomId,
            snapshot.CanvasDataJson,
            snapshot.UpdatedAtUtc);
    }
}
