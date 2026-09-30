using NSubstitute;
using SkillSwapAPI.Application.Common.Interfaces.Identity;
using SkillSwapAPI.Application.Features.Reviews.Queries.GetUserReviews;
using SkillSwapAPI.Application.Features.Users.Dtos;
using SkillSwapAPI.Application.UnitTests.Common;
using SkillSwapAPI.Domain.Modules.Reviews.Entities;
using Xunit;

namespace SkillSwapAPI.Application.UnitTests.Features.Reviews;

public class GetUserReviewsQueryHandlerTests
{
    private readonly UnitOfWorkFixture _fixture = new();
    private readonly IIdentityService _identityService = Substitute.For<IIdentityService>();

    private GetUserReviewsQueryHandler CreateHandler() => new(_fixture.UnitOfWork, _identityService);

    private void SetupReviews(IReadOnlyList<Review> reviews, int totalCount)
    {
        _fixture.Reviews
            .GetPagedByRevieweeAsync(Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<(IReadOnlyList<Review>, int)>((reviews, totalCount)));
    }

    private void SetupProfiles(IReadOnlyList<ProfileIdentityDto> profiles)
    {
        _identityService
            .GetProfilesAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<ProfileIdentityDto>>(profiles));
    }

    [Fact]
    public async Task Handle_ShouldReturnPagedReviewsWithReviewerNames_WhenReviewsExist()
    {
        var reviewerA = Guid.NewGuid();
        var reviewerB = Guid.NewGuid();
        var reviewA = TestData.Review(reviewerId: reviewerA, revieweeId: TestData.UserId, rating: 5, comment: "Excellent");
        var reviewB = TestData.Review(reviewerId: reviewerB, revieweeId: TestData.UserId, rating: 3, comment: null);
        SetupReviews([reviewA, reviewB], 2);
        SetupProfiles(
        [
            TestData.Profile(userId: reviewerA, firstName: "Ada", lastName: "Lovelace"),
            TestData.Profile(userId: reviewerB, firstName: "Grace", lastName: "Hopper")
        ]);

        var query = new GetUserReviewsQuery(TestData.UserId, 1, 10);
        var result = await CreateHandler().Handle(query, CancellationToken.None);

        Assert.False(result.IsError);
        Assert.Equal(2, result.Value.TotalCount);
        Assert.Equal(1, result.Value.PageNumber);
        Assert.Equal(10, result.Value.PageSize);
        Assert.Equal(1, result.Value.TotalPages);
        Assert.Equal(2, result.Value.Items.Count);

        Assert.Equal(reviewA.Id, result.Value.Items[0].Id);
        Assert.Equal(reviewA.SwapRequestId, result.Value.Items[0].SwapRequestId);
        Assert.Equal(reviewerA, result.Value.Items[0].ReviewerId);
        Assert.Equal("Ada", result.Value.Items[0].ReviewerFirstName);
        Assert.Equal("Lovelace", result.Value.Items[0].ReviewerLastName);
        Assert.Equal(TestData.UserId, result.Value.Items[0].RevieweeId);
        Assert.Equal(5, result.Value.Items[0].Rating);
        Assert.Equal("Excellent", result.Value.Items[0].Comment);
        Assert.Equal(reviewA.CreatedAtUtc, result.Value.Items[0].CreatedAtUtc);

        Assert.Equal(reviewB.Id, result.Value.Items[1].Id);
        Assert.Equal(reviewerB, result.Value.Items[1].ReviewerId);
        Assert.Equal("Grace", result.Value.Items[1].ReviewerFirstName);
        Assert.Equal("Hopper", result.Value.Items[1].ReviewerLastName);
        Assert.Equal(3, result.Value.Items[1].Rating);
        Assert.Null(result.Value.Items[1].Comment);

        await _fixture.Reviews.Received(1).GetPagedByRevieweeAsync(
            TestData.UserId, 1, 10, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyItems_WhenUserHasNoReviews()
    {
        SetupReviews([], 0);
        SetupProfiles([]);

        var query = new GetUserReviewsQuery(TestData.UserId, 1, 10);
        var result = await CreateHandler().Handle(query, CancellationToken.None);

        Assert.False(result.IsError);
        Assert.Empty(result.Value.Items);
        Assert.Equal(0, result.Value.TotalCount);
    }

    [Fact]
    public async Task Handle_ShouldUseEmptyReviewerNames_WhenProfileIsNotFound()
    {
        var reviewerA = Guid.NewGuid();
        var reviewA = TestData.Review(reviewerId: reviewerA, revieweeId: TestData.UserId, rating: 4, comment: "Good");
        SetupReviews([reviewA], 1);
        SetupProfiles([]);

        var query = new GetUserReviewsQuery(TestData.UserId);
        var result = await CreateHandler().Handle(query, CancellationToken.None);

        Assert.False(result.IsError);
        var item = Assert.Single(result.Value.Items);
        Assert.Equal(string.Empty, item.ReviewerFirstName);
        Assert.Equal(string.Empty, item.ReviewerLastName);
        Assert.Equal(reviewerA, item.ReviewerId);
        Assert.Equal(4, item.Rating);
    }

    [Fact]
    public async Task Handle_ShouldRequestDistinctReviewerIds_WhenReviewsShareTheSameReviewer()
    {
        var reviewerA = Guid.NewGuid();
        var reviewA = TestData.Review(reviewerId: reviewerA, revieweeId: TestData.UserId, rating: 5);
        var reviewB = TestData.Review(reviewerId: reviewerA, revieweeId: TestData.UserId, rating: 4);
        SetupReviews([reviewA, reviewB], 2);
        SetupProfiles([TestData.Profile(userId: reviewerA, firstName: "Ada", lastName: "Lovelace")]);

        var query = new GetUserReviewsQuery(TestData.UserId, 1, 10);
        var result = await CreateHandler().Handle(query, CancellationToken.None);

        Assert.False(result.IsError);
        Assert.Equal(2, result.Value.Items.Count);
        Assert.Equal("Ada", result.Value.Items[0].ReviewerFirstName);
        Assert.Equal("Ada", result.Value.Items[1].ReviewerFirstName);
        await _identityService.Received(1).GetProfilesAsync(
            Arg.Is<IReadOnlyCollection<Guid>>(ids => ids.Count == 1 && ids.Contains(reviewerA)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldForwardPagingParametersToRepository()
    {
        SetupReviews([], 0);
        SetupProfiles([]);

        var query = new GetUserReviewsQuery(TestData.OtherUserId, 3, 25);
        var result = await CreateHandler().Handle(query, CancellationToken.None);

        Assert.False(result.IsError);
        Assert.Equal(3, result.Value.PageNumber);
        Assert.Equal(25, result.Value.PageSize);
        await _fixture.Reviews.Received(1).GetPagedByRevieweeAsync(
            TestData.OtherUserId, 3, 25, Arg.Any<CancellationToken>());
    }
}
