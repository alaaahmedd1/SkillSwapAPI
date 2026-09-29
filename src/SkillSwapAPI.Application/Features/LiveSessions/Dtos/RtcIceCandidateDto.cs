namespace SkillSwapAPI.Application.Features.LiveSessions.Dtos;

public sealed record RtcIceCandidateDto(
    string Candidate,
    string? SdpMid,
    int? SdpMLineIndex);
