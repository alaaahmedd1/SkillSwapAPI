using NSubstitute;
using SkillSwapAPI.Application.Common.Interfaces.Identity;
using SkillSwapAPI.Application.Features.Reviews.EventHandlers;
using SkillSwapAPI.Application.UnitTests.Common;
using SkillSwapAPI.Domain.Common.Results;
using SkillSwapAPI.Domain.Modules.Reviews.Events;
using Xunit;

namespace SkillSwapAPI.Application.UnitTests.Features.Reviews;

public class ReviewSubmittedEventHandlerTests
{
    private readonly UnitOfWorkFixture _fixture = new();
    private readonly IIdentityService _identityService = Substitute.For<IIdentityService>();

    private ReviewSubmittedEventHandler CreateHandler() => new(_fixture.UnitOfWork, _identityService);

    private static ReviewSubmittedEvent CreateNotification(Guid? revieweeId = null) => new()
    {
        ReviewId = Guid.NewGuid(),
        SwapRequestId = Guid.NewGuid(),
        ReviewerId = TestData.UserId,
        RevieweeId = revieweeId ?? TestData.OtherUserId,
        Rating = 5,
        CreatedAtUtc = DateTimeOffset.UtcNow
    };

    [Fact]
    public async Task Handle_ShouldUpdateRatingSummary_WhenRevieweeHasReviews()
    {
        var notification = CreateNotification();
        _fixture.Reviews
            .GetRatingSummaryAsync(notification.RevieweeId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<(decimal, int)>((4.25m, 8)));
        _identityService
            .UpdateRatingSummaryAsync(Arg.Any<Guid>(), Arg.Any<decimal>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<Updated>.FromSuccess(Result.Updated)));

        await CreateHandler().Handle(notification, CancellationToken.None);

        await _fixture.Reviews.Received(1).GetRatingSummaryAsync(
            notification.RevieweeId, Arg.Any<CancellationToken>());
        await _identityService.Received(1).UpdateRatingSummaryAsync(
            notification.RevieweeId, 4.25m, 8, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldWriteThroughZeroSummary_WhenRevieweeHasNoReviews()
    {
        var notification = CreateNotification();
        _fixture.Reviews
            .GetRatingSummaryAsync(notification.RevieweeId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<(decimal, int)>((0m, 0)));
        _identityService
            .UpdateRatingSummaryAsync(Arg.Any<Guid>(), Arg.Any<decimal>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<Updated>.FromSuccess(Result.Updated)));

        await CreateHandler().Handle(notification, CancellationToken.None);

        await _identityService.Received(1).UpdateRatingSummaryAsync(
            notification.RevieweeId, 0m, 0, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldUpdateSummaryForNotifiedReviewee_WhenMultipleRevieweesExist()
    {
        var notification = CreateNotification(revieweeId: TestData.OtherUserId);
        _fixture.Reviews
            .GetRatingSummaryAsync(TestData.OtherUserId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<(decimal, int)>((3.5m, 2)));
        _fixture.Reviews
            .GetRatingSummaryAsync(TestData.UserId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<(decimal, int)>((1m, 1)));
        _identityService
            .UpdateRatingSummaryAsync(Arg.Any<Guid>(), Arg.Any<decimal>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<Updated>.FromSuccess(Result.Updated)));

        await CreateHandler().Handle(notification, CancellationToken.None);

        await _fixture.Reviews.DidNotReceive().GetRatingSummaryAsync(
            TestData.UserId, Arg.Any<CancellationToken>());
        await _identityService.Received(1).UpdateRatingSummaryAsync(
            TestData.OtherUserId, 3.5m, 2, Arg.Any<CancellationToken>());
    }
}
