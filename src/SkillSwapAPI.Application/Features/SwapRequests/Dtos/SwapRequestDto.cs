using SkillSwapAPI.Domain.Modules.SwapRequests.Enums;

namespace SkillSwapAPI.Application.Features.SwapRequests.Dtos;

public sealed record SwapRequestDto(
    Guid Id,
    Guid RequesterId,
    Guid ReceiverId,
    string RequesterFirstName,
    string RequesterLastName,
    string ReceiverFirstName,
    string ReceiverLastName,
    SwapRequestSkillDto OfferedSkill,
    SwapRequestSkillDto RequestedSkill,
    SwapRequestStatus Status,
    bool IsRequesterConfirmed,
    bool IsReceiverConfirmed,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc,
    Guid? ConversationId);
