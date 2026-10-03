using MediatR;
using SkillSwapAPI.Application.Features.LiveSessions.Dtos;
using SkillSwapAPI.Domain.Common.Results;

namespace SkillSwapAPI.Application.Features.LiveSessions.Queries.GetLiveSessionRoomState;

public sealed record GetLiveSessionRoomStateQuery(
    Guid SwapRequestId,
    Guid UserId) : IRequest<Result<LiveSessionRoomStateDto>>;
