using MediatR;
using SkillSwapAPI.Application.Features.Chat.Dtos;
using SkillSwapAPI.Domain.Common.Results;

namespace SkillSwapAPI.Application.Features.Chat.Commands.SendMessage;

public sealed record SendMessageCommand(
    Guid ConversationId,
    Guid SenderId,
    string Content) : IRequest<Result<MessageDto>>;
