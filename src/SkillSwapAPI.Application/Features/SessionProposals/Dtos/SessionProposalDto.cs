using SkillSwapAPI.Domain.Modules.SessionProposals.Enums;

namespace SkillSwapAPI.Application.Features.SessionProposals.Dtos;

public sealed record SessionProposalDto(
    Guid Id,
    Guid SwapRequestId,
    Guid ProposerId,
    DateOnly ScheduledDate,
    TimeOnly StartTime,
    TimeOnly EndTime,
    int DurationMinutes,
    ProposalStatus Status,
    DateTimeOffset CreatedAtUtc);
