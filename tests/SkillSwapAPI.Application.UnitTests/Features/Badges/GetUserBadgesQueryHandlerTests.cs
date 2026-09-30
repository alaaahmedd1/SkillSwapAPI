using NSubstitute;
using SkillSwapAPI.Application.Features.Badges.Queries.GetUserBadges;
using SkillSwapAPI.Application.UnitTests.Common;
using SkillSwapAPI.Domain.Modules.Badges.Entities;
using Xunit;

namespace SkillSwapAPI.Application.UnitTests.Features.Badges;

public class GetUserBadgesQueryHandlerTests
{
    private readonly UnitOfWorkFixture _fixture = new();

    private void SetupAwards(IReadOnlyList<UserBadgeAward> awards)
    {
        _fixture.UserBadgeAwards
            .GetByRevieweeAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<UserBadgeAward>>(awards));
    }

    private void SetupBadges(IReadOnlyList<Badge> badges)
    {
        _fixture.Badges
            .GetByIdsAsync(Arg.Any<IReadOnlyCollection<int>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<Badge>>(badges));
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenUserHasNoAwards()
    {
        SetupAwards(new List<UserBadgeAward>());

        var handler = new GetUserBadgesQueryHandler(_fixture.UnitOfWork);
        var result = await handler.Handle(new GetUserBadgesQuery(TestData.UserId), CancellationToken.None);

        Assert.False(result.IsError);
        Assert.Empty(result.Value);
        await _fixture.Badges.DidNotReceive().GetByIdsAsync(
            Arg.Any<IReadOnlyCollection<int>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldGroupAwardsByBadge_WhenUserHasMultipleAwardsForSameBadge()
    {
        var olderAward = TestData.UserBadgeAward(badgeId: 1, revieweeId: TestData.UserId);
        olderAward.AwardedAtUtc = DateTimeOffset.UtcNow.AddDays(-2);
        var newerAward = TestData.UserBadgeAward(badgeId: 1, revieweeId: TestData.UserId);
        newerAward.AwardedAtUtc = DateTimeOffset.UtcNow.AddDays(-1);
        var singleAward = TestData.UserBadgeAward(badgeId: 2, revieweeId: TestData.UserId);
        SetupAwards([olderAward, newerAward, singleAward]);
        SetupBadges(
        [
            TestData.Badge(id: 1, name: "Super Patient"),
            TestData.Badge(id: 2, name: "Punctual")
        ]);

        var handler = new GetUserBadgesQueryHandler(_fixture.UnitOfWork);
        var result = await handler.Handle(new GetUserBadgesQuery(TestData.UserId), CancellationToken.None);

        Assert.False(result.IsError);
        Assert.Equal(2, result.Value.Count);

        var patientBadge = result.Value[0];
        Assert.Equal(1, patientBadge.BadgeId);
        Assert.Equal("Super Patient", patientBadge.Name);
        Assert.Equal("Displayed patience during sessions", patientBadge.Description);
        Assert.Equal("https://cdn.skillswap.local/badges/patient.png", patientBadge.IconUrl);
        Assert.Equal(2, patientBadge.AwardCount);
        Assert.Equal(newerAward.AwardedAtUtc, patientBadge.LastAwardedAtUtc);

        var punctualBadge = result.Value[1];
        Assert.Equal(2, punctualBadge.BadgeId);
        Assert.Equal("Punctual", punctualBadge.Name);
        Assert.Equal(1, punctualBadge.AwardCount);
        Assert.Equal(singleAward.AwardedAtUtc, punctualBadge.LastAwardedAtUtc);

        await _fixture.Badges.Received(1).GetByIdsAsync(
            Arg.Is<IReadOnlyCollection<int>>(ids => ids.Count == 2 && ids.Contains(1) && ids.Contains(2)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldOrderBadgesByAwardCountDescendingThenBadgeId_WhenCountsDiffer()
    {
        var badgeOneAward = TestData.UserBadgeAward(badgeId: 1, revieweeId: TestData.UserId);
        var badgeTwoAwardA = TestData.UserBadgeAward(badgeId: 2, revieweeId: TestData.UserId);
        var badgeTwoAwardB = TestData.UserBadgeAward(badgeId: 2, revieweeId: TestData.UserId);
        var badgeThreeAward = TestData.UserBadgeAward(badgeId: 3, revieweeId: TestData.UserId);
        SetupAwards([badgeOneAward, badgeTwoAwardA, badgeTwoAwardB, badgeThreeAward]);
        SetupBadges(
        [
            TestData.Badge(id: 1, name: "Super Patient"),
            TestData.Badge(id: 2, name: "Punctual"),
            TestData.Badge(id: 3, name: "Knowledgeable")
        ]);

        var handler = new GetUserBadgesQueryHandler(_fixture.UnitOfWork);
        var result = await handler.Handle(new GetUserBadgesQuery(TestData.UserId), CancellationToken.None);

        Assert.False(result.IsError);
        Assert.Equal(3, result.Value.Count);
        Assert.Equal(2, result.Value[0].BadgeId);
        Assert.Equal(2, result.Value[0].AwardCount);
        Assert.Equal(1, result.Value[1].BadgeId);
        Assert.Equal(1, result.Value[1].AwardCount);
        Assert.Equal(3, result.Value[2].BadgeId);
        Assert.Equal(1, result.Value[2].AwardCount);
    }

    [Fact]
    public async Task Handle_ShouldIgnoreAwards_WhenBadgeIsMissingFromRepository()
    {
        var knownBadgeAward = TestData.UserBadgeAward(badgeId: 1, revieweeId: TestData.UserId);
        var orphanBadgeAward = TestData.UserBadgeAward(badgeId: 5, revieweeId: TestData.UserId);
        SetupAwards([knownBadgeAward, orphanBadgeAward]);
        SetupBadges([TestData.Badge(id: 1, name: "Super Patient")]);

        var handler = new GetUserBadgesQueryHandler(_fixture.UnitOfWork);
        var result = await handler.Handle(new GetUserBadgesQuery(TestData.UserId), CancellationToken.None);

        Assert.False(result.IsError);
        var item = Assert.Single(result.Value);
        Assert.Equal(1, item.BadgeId);
        Assert.Equal("Super Patient", item.Name);
        Assert.Equal(1, item.AwardCount);

        await _fixture.Badges.Received(1).GetByIdsAsync(
            Arg.Is<IReadOnlyCollection<int>>(ids => ids.Count == 2 && ids.Contains(1) && ids.Contains(5)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldQueryAwardsForRequestedUser()
    {
        SetupAwards(new List<UserBadgeAward>());

        var handler = new GetUserBadgesQueryHandler(_fixture.UnitOfWork);
        await handler.Handle(new GetUserBadgesQuery(TestData.OtherUserId), CancellationToken.None);

        await _fixture.UserBadgeAwards.Received(1).GetByRevieweeAsync(
            TestData.OtherUserId, Arg.Any<CancellationToken>());
    }
}
