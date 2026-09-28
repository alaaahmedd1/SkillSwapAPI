using MediatR;
using SkillSwapAPI.Application.Common.Errors;
using SkillSwapAPI.Application.Common.Interfaces.UnitOfWork;
using SkillSwapAPI.Application.Common.Models;
using SkillSwapAPI.Application.Features.Chat.Dtos;
using SkillSwapAPI.Domain.Common.Results;
using SkillSwapAPI.Domain.Modules.SwapRequests.Enums;

namespace SkillSwapAPI.Application.Features.Chat.Queries.GetChatHistory;

public sealed class GetChatHistoryQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetChatHistoryQuery, Result<PagedResult<MessageDto>>>
{
    public async Task<Result<PagedResult<MessageDto>>> Handle(GetChatHistoryQuery query, CancellationToken ct)
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

        var (messages, totalCount) = await unitOfWork.Messages.GetPagedByConversationAsync(
            query.ConversationId, query.PageNumber, query.PageSize, ct);

        var items = messages.Select(message => new MessageDto(
            message.Id,
            message.ConversationId,
            message.SenderId,
            message.Content,
            message.IsRead,
            message.SentAtUtc)).ToList();

        return PagedResult<MessageDto>.Create(items, totalCount, query.PageNumber, query.PageSize);
    }
}
