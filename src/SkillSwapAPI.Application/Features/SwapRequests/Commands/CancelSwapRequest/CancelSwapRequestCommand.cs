using MediatR;
using SkillSwapAPI.Domain.Common.Results;

namespace SkillSwapAPI.Application.Features.SwapRequests.Commands.CancelSwapRequest;

public sealed record CancelSwapRequestCommand(Guid SwapRequestId, Guid UserId)
    : IRequest<Result<Updated>>;
