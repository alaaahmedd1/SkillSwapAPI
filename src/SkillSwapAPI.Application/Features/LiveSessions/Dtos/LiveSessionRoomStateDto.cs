using SkillSwapAPI.Domain.Modules.LiveSessions.Enums;

namespace SkillSwapAPI.Application.Features.LiveSessions.Dtos;

public sealed record LiveSessionRoomStateDto(
    Guid RoomId,
    LiveSessionStatus Status);
