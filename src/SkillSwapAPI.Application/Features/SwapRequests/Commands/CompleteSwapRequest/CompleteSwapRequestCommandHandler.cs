using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwapAPI.Application.Common.Errors;
using SkillSwapAPI.Application.Common.Interfaces.UnitOfWork;
using SkillSwapAPI.Domain.Common.Results;
using SkillSwapAPI.Domain.Modules.SwapRequests.Enums;

namespace SkillSwapAPI.Application.Features.SwapRequests.Commands.CompleteSwapRequest;

public sealed class CompleteSwapRequestCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<CompleteSwapRequestCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(CompleteSwapRequestCommand command, CancellationToken ct)
    {
        var swapRequest = await unitOfWork.SwapRequests.FindAsync(
            item => item.Id == command.SwapRequestId, ct);

        if (swapRequest is null)
        {
            return ApplicationErrors.SwapRequests.NotFound;
        }

        if (swapRequest.Status != SwapRequestStatus.Accepted)
        {
            return ApplicationErrors.SwapRequests.InvalidStatusTransition;
        }

        if (command.UserId == swapRequest.RequesterId)
        {
            swapRequest.IsRequesterConfirmed = true;
        }
        else if (command.UserId == swapRequest.ReceiverId)
        {
            swapRequest.IsReceiverConfirmed = true;
        }
        else
        {
            return ApplicationErrors.SwapRequests.NotParticipant;
        }

        if (swapRequest.IsRequesterConfirmed && swapRequest.IsReceiverConfirmed)
        {
            swapRequest.Status = SwapRequestStatus.Completed;
            swapRequest.UpdatedAtUtc = DateTimeOffset.UtcNow;
        }

        unitOfWork.SwapRequests.Update(swapRequest);

        try
        {
            await unitOfWork.CompleteAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            return ApplicationErrors.SwapRequests.ConcurrencyConflict;
        }

        return Result.Updated;
    }
}
