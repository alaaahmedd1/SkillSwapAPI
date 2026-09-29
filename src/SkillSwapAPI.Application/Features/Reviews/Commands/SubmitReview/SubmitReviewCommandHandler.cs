using MediatR;
using SkillSwapAPI.Application.Common.Errors;
using SkillSwapAPI.Application.Common.Interfaces.Identity;
using SkillSwapAPI.Application.Common.Interfaces.UnitOfWork;
using SkillSwapAPI.Application.Features.Reviews.Dtos;
using SkillSwapAPI.Domain.Common.Results;
using SkillSwapAPI.Domain.Modules.Badges.Entities;
using SkillSwapAPI.Domain.Modules.Reviews.Entities;
using SkillSwapAPI.Domain.Modules.Reviews.Events;
using SkillSwapAPI.Domain.Modules.SwapRequests.Enums;

namespace SkillSwapAPI.Application.Features.Reviews.Commands.SubmitReview;

public sealed class SubmitReviewCommandHandler(
    IUnitOfWork unitOfWork,
    IIdentityService identityService,
    IPublisher publisher)
    : IRequestHandler<SubmitReviewCommand, Result<ReviewDto>>
{
    public async Task<Result<ReviewDto>> Handle(SubmitReviewCommand command, CancellationToken ct)
    {
        var swapRequest = await unitOfWork.SwapRequests.FindAsync(
            item => item.Id == command.SwapRequestId, ct);

        if (swapRequest is null)
        {
            return ApplicationErrors.SwapRequests.NotFound;
        }

        if (swapRequest.Status != SwapRequestStatus.Completed)
        {
            return ApplicationErrors.Reviews.SwapNotCompleted;
        }

        if (command.ReviewerId != swapRequest.RequesterId && command.ReviewerId != swapRequest.ReceiverId)
        {
            return ApplicationErrors.Reviews.NotParticipant;
        }

        var counterpartId = swapRequest.RequesterId == command.ReviewerId
            ? swapRequest.ReceiverId
            : swapRequest.RequesterId;

        if (command.RevieweeId != counterpartId)
        {
            return ApplicationErrors.Reviews.InvalidReviewee;
        }

        var review = new Review
        {
            Id = Guid.NewGuid(),
            SwapRequestId = swapRequest.Id,
            ReviewerId = command.ReviewerId,
            RevieweeId = command.RevieweeId,
            Rating = command.Rating,
            Comment = command.Comment,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        UserBadgeAward? badgeAward = null;

        if (command.BadgeId.HasValue)
        {
            var badge = await unitOfWork.Badges.GetByIdAsync(command.BadgeId.Value, ct);

            if (badge is null || !badge.IsActive)
            {
                return ApplicationErrors.Badges.BadgeNotFound;
            }

            badgeAward = new UserBadgeAward
            {
                Id = Guid.NewGuid(),
                ReviewId = review.Id,
                BadgeId = badge.Id,
                ReviewerId = command.ReviewerId,
                RevieweeId = command.RevieweeId,
                AwardedAtUtc = DateTimeOffset.UtcNow
            };
        }

        await unitOfWork.Reviews.AddAsync(review, ct);

        if (badgeAward is not null)
        {
            await unitOfWork.UserBadgeAwards.AddAsync(badgeAward, ct);
        }

        await unitOfWork.CompleteAsync(ct);

        await publisher.Publish(new ReviewSubmittedEvent
        {
            ReviewId = review.Id,
            SwapRequestId = review.SwapRequestId,
            ReviewerId = review.ReviewerId,
            RevieweeId = review.RevieweeId,
            Rating = review.Rating,
            CreatedAtUtc = review.CreatedAtUtc
        }, ct);

        var profiles = await identityService.GetProfilesAsync([command.ReviewerId], ct);
        var reviewer = profiles.FirstOrDefault(profile => profile.UserId == command.ReviewerId);

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
    }
}
