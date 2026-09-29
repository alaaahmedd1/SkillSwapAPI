namespace SkillSwapAPI.Application.Features.LiveSessions.Dtos;

public sealed record WhiteboardSnapshotDto(
    Guid Id,
    Guid RoomId,
    string CanvasDataJson,
    DateTimeOffset UpdatedAtUtc);
