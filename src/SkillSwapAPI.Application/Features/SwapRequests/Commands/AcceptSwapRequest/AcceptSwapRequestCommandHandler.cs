using MediatR;
using SkillSwapAPI.Application.Common.Errors;
using SkillSwapAPI.Application.Common.Interfaces.UnitOfWork;
using SkillSwapAPI.Domain.Common.Results;
using SkillSwapAPI.Domain.Modules.Chat.Entities;
using SkillSwapAPI.Domain.Modules.SwapRequests.Enums;

namespace SkillSwapAPI.Application.Features.SwapRequests.Commands.AcceptSwapRequest;

public sealed class AcceptSwapRequestCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<AcceptSwapRequestCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(AcceptSwapRequestCommand command, CancellationToken ct)
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

        swapRequest.Status = SwapRequestStatus.Accepted;
        swapRequest.UpdatedAtUtc = DateTimeOffset.UtcNow;

        unitOfWork.SwapRequests.Update(swapRequest);

        await unitOfWork.Conversations.AddAsync(new Conversation
        {
            Id = Guid.NewGuid(),
            SwapRequestId = swapRequest.Id,
            CreatedAtUtc = DateTimeOffset.UtcNow
        }, ct);

        await unitOfWork.CompleteAsync(ct);

        return Result.Updated;
    }
}
