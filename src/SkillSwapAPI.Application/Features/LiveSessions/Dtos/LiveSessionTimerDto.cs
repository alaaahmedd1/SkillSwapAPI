namespace SkillSwapAPI.Application.Features.LiveSessions.Dtos;

public sealed record LiveSessionTimerDto(
    DateTimeOffset? StartedAtUtc,
    int ElapsedSeconds);
