using MediatR;
using SkillSwapAPI.Application.Common.Errors;
using SkillSwapAPI.Application.Common.Interfaces.Services;
using SkillSwapAPI.Application.Common.Interfaces.UnitOfWork;
using SkillSwapAPI.Application.Features.LiveSessions.Dtos;
using SkillSwapAPI.Domain.Common.Results;
using SkillSwapAPI.Domain.Modules.LiveSessions.Entities;
using SkillSwapAPI.Domain.Modules.LiveSessions.Enums;

namespace SkillSwapAPI.Application.Features.LiveSessions.Commands.EndLiveSession;

public sealed class EndLiveSessionCommandHandler(
    IUnitOfWork unitOfWork,
    ITimeLedgerService timeLedgerService)
    : IRequestHandler<EndLiveSessionCommand, Result<LiveSessionRoomDto>>
{
    public async Task<Result<LiveSessionRoomDto>> Handle(EndLiveSessionCommand command, CancellationToken ct)
    {
        var room = await unitOfWork.LiveSessionRooms.GetByIdAsync(command.RoomId);

        if (room is null)
        {
            return ApplicationErrors.LiveSessions.RoomNotFound;
        }

        var swapRequest = await unitOfWork.SwapRequests.FindAsync(
            item => item.Id == room.SwapRequestId, ct);

        if (swapRequest is null)
        {
            return ApplicationErrors.SwapRequests.NotFound;
        }

        if (swapRequest.RequesterId != command.UserId && swapRequest.ReceiverId != command.UserId)
        {
            return ApplicationErrors.LiveSessions.NotParticipant;
        }

        if (room.Status == LiveSessionStatus.Ended)
        {
            return ToDto(room);
        }

        var now = DateTimeOffset.UtcNow;
        var durationSeconds = 0;
        var settledMinutes = 0;

        if (room.Status == LiveSessionStatus.InProgress && room.ActualStartTime.HasValue)
        {
            durationSeconds = (int)Math.Max(0, (now - room.ActualStartTime.Value).TotalSeconds);
            settledMinutes = (int)Math.Round(durationSeconds / 60.0, MidpointRounding.AwayFromZero);
        }

        // A revived room (rejoined after ending) settles only once per swap; the first
        // end already wrote the ledger rows, so later ends just close the room.
        var alreadySettled = await unitOfWork.TimeLedgerTransactions.ExistsForSwapRequestAsync(
            swapRequest.Id, ct);

        if (settledMinutes > 0 && !alreadySettled)
        {
            try
            {
                await timeLedgerService.ValidateSettlementAsync(
                    swapRequest.RequesterId,
                    swapRequest.ReceiverId,
                    settledMinutes,
                    ct);
            }
            catch (KeyNotFoundException)
            {
                return ApplicationErrors.LiveSessions.WalletNotFound;
            }
            catch (InvalidOperationException)
            {
                return ApplicationErrors.LiveSessions.InsufficientBalance;
            }

            try
            {
                await timeLedgerService.SettleAsync(
                    swapRequest.RequesterId,
                    swapRequest.ReceiverId,
                    settledMinutes,
                    swapRequest.Id,
                    ct);
            }
            catch (InvalidOperationException)
            {
                return ApplicationErrors.LiveSessions.SettlementFailed;
            }
            catch (KeyNotFoundException)
            {
                return ApplicationErrors.LiveSessions.WalletNotFound;
            }
        }

        room.Status = LiveSessionStatus.Ended;
        room.ActualEndTime = now;
        room.DurationSeconds = durationSeconds;

        unitOfWork.LiveSessionRooms.Update(room);
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
