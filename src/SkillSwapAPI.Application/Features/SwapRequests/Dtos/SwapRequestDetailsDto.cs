using SkillSwapAPI.Domain.Modules.SwapRequests.Enums;

namespace SkillSwapAPI.Application.Features.SwapRequests.Dtos;

public sealed record SwapRequestDetailsDto(
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
    string? ProposedScheduleDetails,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc);
