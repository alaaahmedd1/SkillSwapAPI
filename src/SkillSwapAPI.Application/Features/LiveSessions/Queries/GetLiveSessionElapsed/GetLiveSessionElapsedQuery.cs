using MediatR;
using SkillSwapAPI.Application.Features.LiveSessions.Dtos;
using SkillSwapAPI.Domain.Common.Results;

namespace SkillSwapAPI.Application.Features.LiveSessions.Queries.GetLiveSessionElapsed;

public sealed record GetLiveSessionElapsedQuery(
    Guid SwapRequestId,
    Guid UserId) : IRequest<Result<LiveSessionTimerDto>>;
