namespace SkillSwapAPI.Application.Features.LiveSessions.Dtos;

public sealed record RtcSessionDescriptionDto(
    string Type,
    string Sdp);
