using MediatR;
using SkillSwapAPI.Application.Common.Errors;
using SkillSwapAPI.Application.Common.Interfaces.UnitOfWork;
using SkillSwapAPI.Domain.Common.Results;
using SkillSwapAPI.Domain.Modules.SwapRequests.Enums;

namespace SkillSwapAPI.Application.Features.Chat.Queries.GetConversationAccess;

public sealed class GetConversationAccessQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetConversationAccessQuery, Result<Success>>
{
    public async Task<Result<Success>> Handle(GetConversationAccessQuery query, CancellationToken ct)
    {
        var swapRequest = await unitOfWork.Conversations.GetSwapRequestByConversationAsync(query.ConversationId, ct);

        if (swapRequest is null)
        {
            return ApplicationErrors.Chat.ConversationNotFound;
        }

        if (swapRequest.RequesterId != query.UserId && swapRequest.ReceiverId != query.UserId)
        {
            return ApplicationErrors.Chat.NotSwapParticipant;
        }

        if (swapRequest.Status != SwapRequestStatus.Accepted && swapRequest.Status != SwapRequestStatus.Completed)
        {
            return ApplicationErrors.Chat.SwapNotActive;
        }

        return Result.Success;
    }
}
