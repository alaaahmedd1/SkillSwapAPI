using MediatR;
using SkillSwapAPI.Application.Common.Errors;
using SkillSwapAPI.Application.Common.Interfaces.UnitOfWork;
using SkillSwapAPI.Domain.Common.Results;
using SkillSwapAPI.Domain.Modules.SwapRequests.Enums;

namespace SkillSwapAPI.Application.Features.SwapRequests.Commands.CancelSwapRequest;

public sealed class CancelSwapRequestCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<CancelSwapRequestCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(CancelSwapRequestCommand command, CancellationToken ct)
    {
        var swapRequest = await unitOfWork.SwapRequests.FindAsync(
            item => item.Id == command.SwapRequestId, ct);

        if (swapRequest is null)
        {
            return ApplicationErrors.SwapRequests.NotFound;
        }

        if (swapRequest.Status == SwapRequestStatus.Pending)
        {
            if (swapRequest.RequesterId != command.UserId)
            {
                return ApplicationErrors.SwapRequests.OnlyRequesterCanCancel;
            }
        }
        else if (swapRequest.Status == SwapRequestStatus.Accepted)
        {
            if (swapRequest.RequesterId != command.UserId && swapRequest.ReceiverId != command.UserId)
            {
                return ApplicationErrors.SwapRequests.NotParticipant;
            }
        }
        else
        {
            return ApplicationErrors.SwapRequests.InvalidStatusTransition;
        }

        swapRequest.Status = SwapRequestStatus.Cancelled;
        swapRequest.UpdatedAtUtc = DateTimeOffset.UtcNow;

        unitOfWork.SwapRequests.Update(swapRequest);
        await unitOfWork.CompleteAsync(ct);

        return Result.Updated;
    }
}
