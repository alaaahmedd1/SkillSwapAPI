namespace SkillSwapAPI.Application.Features.Chat.Dtos;

public sealed record MessageDto(
    Guid Id,
    Guid ConversationId,
    Guid SenderId,
    string Content,
    bool IsRead,
    DateTimeOffset SentAtUtc);
