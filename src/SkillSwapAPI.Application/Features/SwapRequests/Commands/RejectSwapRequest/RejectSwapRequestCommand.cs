using MediatR;
using SkillSwapAPI.Domain.Common.Results;

namespace SkillSwapAPI.Application.Features.SwapRequests.Commands.RejectSwapRequest;

public sealed record RejectSwapRequestCommand(Guid SwapRequestId, Guid UserId)
    : IRequest<Result<Updated>>;
