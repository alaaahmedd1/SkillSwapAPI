using MediatR;
using SkillSwapAPI.Application.Common.Interfaces.Identity;
using SkillSwapAPI.Application.Common.Interfaces.UnitOfWork;
using SkillSwapAPI.Domain.Modules.Reviews.Events;

namespace SkillSwapAPI.Application.Features.Reviews.EventHandlers;

public sealed class ReviewSubmittedEventHandler(
    IUnitOfWork unitOfWork,
    IIdentityService identityService)
    : INotificationHandler<ReviewSubmittedEvent>
{
    public async Task Handle(ReviewSubmittedEvent notification, CancellationToken ct)
    {
        var (averageRating, totalReviewsCount) = await unitOfWork.Reviews.GetRatingSummaryAsync(
            notification.RevieweeId, ct);

        await identityService.UpdateRatingSummaryAsync(
            notification.RevieweeId, averageRating, totalReviewsCount, ct);
    }
}
