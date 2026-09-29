using MediatR;
using SkillSwapAPI.Application.Features.LiveSessions.Dtos;
using SkillSwapAPI.Domain.Common.Results;

namespace SkillSwapAPI.Application.Features.LiveSessions.Commands.JoinLiveSession;

public sealed record JoinLiveSessionCommand(Guid SwapRequestId, Guid UserId)
    : IRequest<Result<LiveSessionRoomDto>>;
