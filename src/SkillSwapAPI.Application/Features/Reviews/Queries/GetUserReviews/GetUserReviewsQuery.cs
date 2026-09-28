using MediatR;
using SkillSwapAPI.Application.Common.Models;
using SkillSwapAPI.Application.Features.Reviews.Dtos;
using SkillSwapAPI.Domain.Common.Results;

namespace SkillSwapAPI.Application.Features.Reviews.Queries.GetUserReviews;

public sealed record GetUserReviewsQuery(
    Guid UserId,
    int PageNumber = 1,
    int PageSize = 10) : IRequest<Result<PagedResult<ReviewDto>>>;
