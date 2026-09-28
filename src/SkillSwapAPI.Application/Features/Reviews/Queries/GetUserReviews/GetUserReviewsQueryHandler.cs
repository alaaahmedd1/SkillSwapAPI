using MediatR;
using SkillSwapAPI.Application.Common.Interfaces.Identity;
using SkillSwapAPI.Application.Common.Interfaces.UnitOfWork;
using SkillSwapAPI.Application.Common.Models;
using SkillSwapAPI.Application.Features.Reviews.Dtos;
using SkillSwapAPI.Domain.Common.Results;

namespace SkillSwapAPI.Application.Features.Reviews.Queries.GetUserReviews;

public sealed class GetUserReviewsQueryHandler(
    IUnitOfWork unitOfWork,
    IIdentityService identityService)
    : IRequestHandler<GetUserReviewsQuery, Result<PagedResult<ReviewDto>>>
{
    public async Task<Result<PagedResult<ReviewDto>>> Handle(GetUserReviewsQuery query, CancellationToken ct)
    {
        var (pageItems, totalCount) = await unitOfWork.Reviews.GetPagedByRevieweeAsync(
            query.UserId, query.PageNumber, query.PageSize, ct);

        var reviewerIds = pageItems
            .Select(review => review.ReviewerId)
            .Distinct()
            .ToList();

        var profiles = await identityService.GetProfilesAsync(reviewerIds, ct);

        var items = pageItems
            .Select(review =>
            {
                var reviewer = profiles.FirstOrDefault(profile => profile.UserId == review.ReviewerId);

                return new ReviewDto(
                    review.Id,
                    review.SwapRequestId,
                    review.ReviewerId,
                    reviewer?.FirstName ?? string.Empty,
                    reviewer?.LastName ?? string.Empty,
                    review.RevieweeId,
                    review.Rating,
                    review.Comment,
                    review.CreatedAtUtc);
            })
            .ToList();

        return PagedResult<ReviewDto>.Create(items, totalCount, query.PageNumber, query.PageSize);
    }
}
