using MediatR;
using SkillSwapAPI.Application.Common.Errors;
using SkillSwapAPI.Application.Common.Interfaces.UnitOfWork;
using SkillSwapAPI.Domain.Common.Results;
using SkillSwapAPI.Domain.Modules.SwapRequests.Enums;

namespace SkillSwapAPI.Application.Features.SwapRequests.Commands.RejectSwapRequest;

public sealed class RejectSwapRequestCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<RejectSwapRequestCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(RejectSwapRequestCommand command, CancellationToken ct)
    {
        var swapRequest = await unitOfWork.SwapRequests.FindAsync(
            item => item.Id == command.SwapRequestId, ct);

        if (swapRequest is null)
        {
            return ApplicationErrors.SwapRequests.NotFound;
        }

        if (swapRequest.Status != SwapRequestStatus.Pending)
        {
            return ApplicationErrors.SwapRequests.InvalidStatusTransition;
        }

        if (swapRequest.ReceiverId != command.UserId)
        {
            return ApplicationErrors.SwapRequests.OnlyReceiverCanRespond;
        }

        swapRequest.Status = SwapRequestStatus.Rejected;
        swapRequest.UpdatedAtUtc = DateTimeOffset.UtcNow;

        unitOfWork.SwapRequests.Update(swapRequest);
        await unitOfWork.CompleteAsync(ct);

        return Result.Updated;
    }
}
