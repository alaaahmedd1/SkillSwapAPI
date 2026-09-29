using MediatR;
using SkillSwapAPI.Domain.Common.Results;

namespace SkillSwapAPI.Application.Features.LiveSessions.Queries.GetLiveSessionAccess;

public sealed record GetLiveSessionAccessQuery(
    Guid SwapRequestId,
    Guid UserId) : IRequest<Result<Success>>;
