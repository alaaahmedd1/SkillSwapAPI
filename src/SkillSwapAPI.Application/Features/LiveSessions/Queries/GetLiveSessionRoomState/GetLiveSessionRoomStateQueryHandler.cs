using MediatR;
using SkillSwapAPI.Application.Common.Errors;
using SkillSwapAPI.Application.Common.Interfaces.UnitOfWork;
using SkillSwapAPI.Application.Features.LiveSessions.Dtos;
using SkillSwapAPI.Domain.Common.Results;
using SkillSwapAPI.Domain.Modules.SwapRequests.Enums;

namespace SkillSwapAPI.Application.Features.LiveSessions.Queries.GetLiveSessionRoomState;

public sealed class GetLiveSessionRoomStateQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetLiveSessionRoomStateQuery, Result<LiveSessionRoomStateDto>>
{
    public async Task<Result<LiveSessionRoomStateDto>> Handle(GetLiveSessionRoomStateQuery query, CancellationToken ct)
    {
        var swapRequest = await unitOfWork.SwapRequests.FindAsync(
            item => item.Id == query.SwapRequestId, ct);

        if (swapRequest is null)
        {
            return ApplicationErrors.SwapRequests.NotFound;
        }

        if (swapRequest.Status != SwapRequestStatus.Accepted)
        {
            return ApplicationErrors.LiveSessions.SwapNotAccepted;
        }

        if (swapRequest.RequesterId != query.UserId && swapRequest.ReceiverId != query.UserId)
        {
            return ApplicationErrors.LiveSessions.NotParticipant;
        }

        var room = await unitOfWork.LiveSessionRooms.GetBySwapRequestIdAsync(query.SwapRequestId, ct);

        if (room is null)
        {
            return ApplicationErrors.LiveSessions.RoomNotFound;
        }

        return new LiveSessionRoomStateDto(room.Id, room.Status);
    }
}
