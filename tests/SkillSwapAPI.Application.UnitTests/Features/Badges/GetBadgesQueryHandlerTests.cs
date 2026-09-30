using NSubstitute;
using SkillSwapAPI.Application.Features.Badges.Queries.GetBadges;
using SkillSwapAPI.Application.UnitTests.Common;
using SkillSwapAPI.Domain.Modules.Badges.Entities;
using Xunit;

namespace SkillSwapAPI.Application.UnitTests.Features.Badges;

public class GetBadgesQueryHandlerTests
{
    private readonly UnitOfWorkFixture _fixture = new();

    [Fact]
    public async Task Handle_ShouldReturnBadgeDtos_WhenActiveBadgesExist()
    {
        var badges = new List<Badge>
        {
            TestData.Badge(id: 1, name: "Super Patient"),
            TestData.Badge(id: 2, name: "Punctual")
        };
        _fixture.Badges
            .GetActiveAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<Badge>>(badges));

        var handler = new GetBadgesQueryHandler(_fixture.UnitOfWork);
        var result = await handler.Handle(new GetBadgesQuery(), CancellationToken.None);

        Assert.False(result.IsError);
        Assert.Equal(2, result.Value.Count);

        Assert.Equal(1, result.Value[0].Id);
        Assert.Equal("Super Patient", result.Value[0].Name);
        Assert.Equal("Displayed patience during sessions", result.Value[0].Description);
        Assert.Equal("https://cdn.skillswap.local/badges/patient.png", result.Value[0].IconUrl);

        Assert.Equal(2, result.Value[1].Id);
        Assert.Equal("Punctual", result.Value[1].Name);

        await _fixture.Badges.Received(1).GetActiveAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyList_WhenNoActiveBadgesExist()
    {
        _fixture.Badges
            .GetActiveAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<Badge>>(new List<Badge>()));

        var handler = new GetBadgesQueryHandler(_fixture.UnitOfWork);
        var result = await handler.Handle(new GetBadgesQuery(), CancellationToken.None);

        Assert.False(result.IsError);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task Handle_ShouldPreserveRepositoryOrder_WhenMappingBadges()
    {
        var badges = new List<Badge>
        {
            TestData.Badge(id: 5, name: "Fifth"),
            TestData.Badge(id: 3, name: "Third"),
            TestData.Badge(id: 9, name: "Ninth")
        };
        _fixture.Badges
            .GetActiveAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<Badge>>(badges));

        var handler = new GetBadgesQueryHandler(_fixture.UnitOfWork);
        var result = await handler.Handle(new GetBadgesQuery(), CancellationToken.None);

        Assert.False(result.IsError);
        Assert.Equal(3, result.Value.Count);
        Assert.Equal(5, result.Value[0].Id);
        Assert.Equal(3, result.Value[1].Id);
        Assert.Equal(9, result.Value[2].Id);
    }
}
