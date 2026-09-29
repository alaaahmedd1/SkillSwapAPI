using MediatR;
using SkillSwapAPI.Application.Common.Errors;
using SkillSwapAPI.Application.Common.Interfaces.UnitOfWork;
using SkillSwapAPI.Domain.Common.Results;
using SkillSwapAPI.Domain.Modules.LiveSessions.Enums;
using SkillSwapAPI.Domain.Modules.SwapRequests.Enums;

namespace SkillSwapAPI.Application.Features.LiveSessions.Queries.GetLiveSessionAccess;

public sealed class GetLiveSessionAccessQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetLiveSessionAccessQuery, Result<Success>>
{
    public async Task<Result<Success>> Handle(GetLiveSessionAccessQuery query, CancellationToken ct)
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

        if (room.Status == LiveSessionStatus.Ended)
        {
            return ApplicationErrors.LiveSessions.RoomAlreadyEnded;
        }

        return Result.Success;
    }
}
