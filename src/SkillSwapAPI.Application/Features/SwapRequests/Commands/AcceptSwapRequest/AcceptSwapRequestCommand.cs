using MediatR;
using SkillSwapAPI.Domain.Common.Results;

namespace SkillSwapAPI.Application.Features.SwapRequests.Commands.AcceptSwapRequest;

public sealed record AcceptSwapRequestCommand(Guid SwapRequestId, Guid UserId)
    : IRequest<Result<Updated>>;
