using MediatR;
using SkillSwapAPI.Application.Common.Models;
using SkillSwapAPI.Application.Features.SwapRequests.Dtos;
using SkillSwapAPI.Domain.Common.Results;
using SkillSwapAPI.Domain.Modules.SwapRequests.Enums;

namespace SkillSwapAPI.Application.Features.SwapRequests.Queries.GetSwapRequests;

public sealed record GetSwapRequestsQuery(
    Guid UserId,
    SwapRequestStatus? Status = null,
    int PageNumber = 1,
    int PageSize = 10) : IRequest<Result<PagedResult<SwapRequestDto>>>;
