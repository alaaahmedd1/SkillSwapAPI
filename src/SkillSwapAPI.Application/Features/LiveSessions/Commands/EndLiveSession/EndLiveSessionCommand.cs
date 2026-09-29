using MediatR;
using SkillSwapAPI.Application.Features.LiveSessions.Dtos;
using SkillSwapAPI.Domain.Common.Results;

namespace SkillSwapAPI.Application.Features.LiveSessions.Commands.EndLiveSession;

public sealed record EndLiveSessionCommand(
    Guid RoomId,
    Guid UserId) : IRequest<Result<LiveSessionRoomDto>>;
