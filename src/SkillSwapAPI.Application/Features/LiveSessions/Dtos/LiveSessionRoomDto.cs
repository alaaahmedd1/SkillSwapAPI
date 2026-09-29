using SkillSwapAPI.Domain.Modules.LiveSessions.Enums;

namespace SkillSwapAPI.Application.Features.LiveSessions.Dtos;

public sealed record LiveSessionRoomDto(
    Guid Id,
    Guid SwapRequestId,
    string RoomToken,
    DateTimeOffset ScheduledStartTime,
    DateTimeOffset? ActualStartTime,
    DateTimeOffset? ActualEndTime,
    int DurationSeconds,
    LiveSessionStatus Status);
