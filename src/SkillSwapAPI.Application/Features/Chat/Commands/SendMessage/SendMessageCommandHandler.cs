using MediatR;
using SkillSwapAPI.Application.Common.Errors;
using SkillSwapAPI.Application.Common.Interfaces.UnitOfWork;
using SkillSwapAPI.Application.Features.Chat.Dtos;
using SkillSwapAPI.Domain.Common.Results;
using SkillSwapAPI.Domain.Modules.Chat.Entities;
using SkillSwapAPI.Domain.Modules.SwapRequests.Enums;

namespace SkillSwapAPI.Application.Features.Chat.Commands.SendMessage;

public sealed class SendMessageCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<SendMessageCommand, Result<MessageDto>>
{
    public async Task<Result<MessageDto>> Handle(SendMessageCommand command, CancellationToken ct)
    {
        var swapRequest = await unitOfWork.Conversations.GetSwapRequestByConversationAsync(command.ConversationId, ct);

        if (swapRequest is null)
        {
            return ApplicationErrors.Chat.ConversationNotFound;
        }

        if (swapRequest.RequesterId != command.SenderId && swapRequest.ReceiverId != command.SenderId)
        {
            return ApplicationErrors.Chat.NotSwapParticipant;
        }

        if (swapRequest.Status != SwapRequestStatus.Accepted && swapRequest.Status != SwapRequestStatus.Completed)
        {
            return ApplicationErrors.Chat.SwapNotActive;
        }

        var message = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = command.ConversationId,
            SenderId = command.SenderId,
            Content = command.Content,
            IsRead = false,
            SentAtUtc = DateTimeOffset.UtcNow
        };

        await unitOfWork.Messages.AddAsync(message, ct);
        await unitOfWork.CompleteAsync(ct);

        return new MessageDto(
            message.Id,
            message.ConversationId,
            message.SenderId,
            message.Content,
            message.IsRead,
            message.SentAtUtc);
    }
}
