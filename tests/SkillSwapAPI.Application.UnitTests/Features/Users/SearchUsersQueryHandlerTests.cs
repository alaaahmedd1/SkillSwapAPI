using NSubstitute;
using SkillSwapAPI.Application.Common.Interfaces.Identity;
using SkillSwapAPI.Application.Features.Users.Dtos;
using SkillSwapAPI.Application.Features.Users.Queries.SearchUsers;
using SkillSwapAPI.Application.UnitTests.Common;
using SkillSwapAPI.Domain.Modules.Users.Enums;
using Xunit;

namespace SkillSwapAPI.Application.UnitTests.Features.Users;

public sealed class SearchUsersQueryHandlerTests
{
    [Fact]
    public async Task Handle_FiltersInactiveAndLowRatedUsersThenSortsAndPagesMatches()
    {
        var fixture = new UnitOfWorkFixture();
        var identity = Substitute.For<IIdentityService>();
        var lowRatingId = Guid.NewGuid();
        var inactiveId = Guid.NewGuid();
        var topId = Guid.NewGuid();
        var nextId = Guid.NewGuid();
        fixture.UserSkills.GetUserIdsByFiltersAsync(1, null, null, ProficiencyLevel.Expert, Arg.Any<CancellationToken>())
            .Returns([lowRatingId, inactiveId, nextId, topId]);
        identity.GetProfilesAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>()).Returns(
        [
            Profile(lowRatingId, "Barbara", "Jones", 2.9m, true),
            Profile(inactiveId, "Alex", "Smith", 5m, false),
            Profile(nextId, "Aaron", "Lee", 4.5m, true),
            Profile(topId, "Ada", "Lovelace", 5m, true)
        ]);
        var matchingSkill = TestData.UserSkill(userId: topId, skillId: Guid.NewGuid(), type: SkillType.Offered, level: ProficiencyLevel.Expert);
        fixture.UserSkills.GetByUserIdsAsync(Arg.Is<IReadOnlyCollection<Guid>>(ids => ids.SequenceEqual(new[] { topId })), Arg.Any<CancellationToken>())
            .Returns([matchingSkill]);

        var query = new SearchUsersQuery("a", CategoryId: 1, MinRating: 3m, ProficiencyLevel: ProficiencyLevel.Expert, PageNumber: 1, PageSize: 1);
        var result = await new SearchUsersQueryHandler(fixture.UnitOfWork, identity).Handle(query, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.TotalCount);
        Assert.Single(result.Value.Items);
        Assert.Equal(topId, result.Value.Items[0].UserId);
        Assert.Equal("Ada", result.Value.Items[0].FirstName);
        Assert.Equal(matchingSkill.Skill.Name, result.Value.Items[0].Skills.Single().SkillName);
    }

    private static ProfileIdentityDto Profile(Guid id, string firstName, string lastName, decimal rating, bool active) =>
        new(id, firstName, lastName, $"{id:N}@example.com", rating, 2, active, DateTimeOffset.UtcNow);
}
