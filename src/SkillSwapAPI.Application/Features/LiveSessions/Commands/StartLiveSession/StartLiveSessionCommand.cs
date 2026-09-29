using MediatR;
using SkillSwapAPI.Application.Features.LiveSessions.Dtos;
using SkillSwapAPI.Domain.Common.Results;

namespace SkillSwapAPI.Application.Features.LiveSessions.Commands.StartLiveSession;

public sealed record StartLiveSessionCommand(
    Guid SwapRequestId,
    Guid UserId) : IRequest<Result<LiveSessionRoomDto>>;
