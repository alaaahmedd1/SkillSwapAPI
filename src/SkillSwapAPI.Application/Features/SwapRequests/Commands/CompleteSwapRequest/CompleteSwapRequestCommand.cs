using MediatR;
using SkillSwapAPI.Domain.Common.Results;

namespace SkillSwapAPI.Application.Features.SwapRequests.Commands.CompleteSwapRequest;

public sealed record CompleteSwapRequestCommand(Guid SwapRequestId, Guid UserId)
    : IRequest<Result<Updated>>;
