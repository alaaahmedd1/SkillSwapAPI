using MediatR;
using SkillSwapAPI.Application.Features.SwapRequests.Dtos;
using SkillSwapAPI.Domain.Common.Results;

namespace SkillSwapAPI.Application.Features.SwapRequests.Queries.GetSwapRequestDetails;

public sealed record GetSwapRequestDetailsQuery(Guid SwapRequestId, Guid UserId)
    : IRequest<Result<SwapRequestDetailsDto>>;
