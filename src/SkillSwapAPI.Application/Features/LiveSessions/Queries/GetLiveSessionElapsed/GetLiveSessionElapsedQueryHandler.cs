using MediatR;
using SkillSwapAPI.Application.Common.Errors;
using SkillSwapAPI.Application.Common.Interfaces.UnitOfWork;
using SkillSwapAPI.Application.Features.LiveSessions.Dtos;
using SkillSwapAPI.Domain.Common.Results;
using SkillSwapAPI.Domain.Modules.LiveSessions.Enums;
using SkillSwapAPI.Domain.Modules.SwapRequests.Enums;

namespace SkillSwapAPI.Application.Features.LiveSessions.Queries.GetLiveSessionElapsed;

public sealed class GetLiveSessionElapsedQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetLiveSessionElapsedQuery, Result<LiveSessionTimerDto>>
{
    public async Task<Result<LiveSessionTimerDto>> Handle(GetLiveSessionElapsedQuery query, CancellationToken ct)
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

        if (room.Status == LiveSessionStatus.Ended || !room.ActualStartTime.HasValue)
        {
            return new LiveSessionTimerDto(room.ActualStartTime, 0);
        }

        var elapsedSeconds = (int)Math.Max(0, (DateTimeOffset.UtcNow - room.ActualStartTime.Value).TotalSeconds);

        return new LiveSessionTimerDto(room.ActualStartTime, elapsedSeconds);
    }
}
