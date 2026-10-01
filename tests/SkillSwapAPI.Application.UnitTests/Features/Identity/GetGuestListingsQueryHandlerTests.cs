using NSubstitute;
using SkillSwapAPI.Application.Common.Interfaces.Identity;
using SkillSwapAPI.Application.Common.Interfaces.Repos;
using SkillSwapAPI.Application.Common.Interfaces.UnitOfWork;
using SkillSwapAPI.Application.Features.Identity.Queries.GetGuestListings;
using SkillSwapAPI.Application.Features.Users.Dtos;
using SkillSwapAPI.Domain.Modules.Users.Entities;
using SkillSwapAPI.Domain.Modules.Users.Enums;
using SkillSwapAPI.Domain.Skills.Entities;
using Xunit;

namespace SkillSwapAPI.Application.UnitTests.Features.Identity;

public sealed class GetGuestListingsQueryHandlerTests
{
    private readonly IUnitOfWork unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IUserSkillRepository userSkills = Substitute.For<IUserSkillRepository>();
    private readonly IIdentityService identityService = Substitute.For<IIdentityService>();

    private static UserSkill OfferedSkill(Guid userId, string name) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        SkillId = Guid.NewGuid(),
        Type = SkillType.Offered,
        Skill = new Skill { Id = Guid.NewGuid(), Name = name }
    };

    private static UserSkill SeekingSkill(Guid userId, string name) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        SkillId = Guid.NewGuid(),
        Type = SkillType.Seeking,
        Skill = new Skill { Id = Guid.NewGuid(), Name = name }
    };

    private static ProfileIdentityDto Profile(Guid userId, string first, string last) => new(
        userId, first, last, $"{first}@example.com", 4.5m, 2, true, DateTimeOffset.UtcNow);

    private GetGuestListingsQueryHandler CreateHandler()
    {
        unitOfWork.UserSkills.Returns(userSkills);
        return new GetGuestListingsQueryHandler(unitOfWork, identityService);
    }

    [Fact]
    public async Task Handle_ReturnsOfferedAndWantedSkillsWithProfileData()
    {
        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();

        userSkills.GetUserIdsByFiltersAsync(
                Arg.Any<int?>(), Arg.Any<Guid?>(), Arg.Any<Guid?>(), Arg.Any<ProficiencyLevel?>(),
                Arg.Any<CancellationToken>())
            .Returns([userA, userB]);

        userSkills.GetByUserIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns([
                OfferedSkill(userA, "Figma"),
                SeekingSkill(userA, "Spanish"),
                OfferedSkill(userB, "C#"),
            ]);

        identityService.GetProfilesAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns([
                Profile(userA, "Aya", "Mohamed"),
                Profile(userB, "Nesreen", "Ahmed"),
            ]);

        var result = await CreateHandler()
            .Handle(new GetGuestListingsQuery(PageNumber: 1, PageSize: 10), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.TotalCount);

        var first = result.Value.Items[0];
        Assert.Contains(first.SkillsOffered, skill => skill == "Figma");
        Assert.Contains(first.SkillsWanted, skill => skill == "Spanish");

        var second = result.Value.Items[1];
        Assert.Contains(second.SkillsOffered, skill => skill == "C#");
        Assert.Empty(second.SkillsWanted);
    }

    [Fact]
    public async Task Handle_WithSkillFilter_ReturnsOnlyMatchingPublicListing()
    {
        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();

        userSkills.GetUserIdsByFiltersAsync(
                Arg.Any<int?>(), Arg.Any<Guid?>(), Arg.Any<Guid?>(), Arg.Any<ProficiencyLevel?>(),
                Arg.Any<CancellationToken>())
            .Returns([userA, userB]);

        userSkills.GetByUserIdsAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns([
                OfferedSkill(userA, "English Conversation"),
                OfferedSkill(userB, "C#"),
            ]);

        identityService.GetProfilesAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns([
                Profile(userA, "Nesreen", "Ahmed"),
                Profile(userB, "Other", "User"),
            ]);

        var result = await CreateHandler()
            .Handle(new GetGuestListingsQuery(SkillFilter: "english"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Value.TotalCount);
        Assert.Single(result.Value.Items);
        Assert.Equal("Nesreen Ahmed", result.Value.Items[0].DisplayName);
        Assert.DoesNotContain(result.Value.Items, item => item.SkillsOffered.Any(
            skill => skill.Contains("C#", StringComparison.OrdinalIgnoreCase)));
    }

    [Fact]
    public async Task Handle_WithNoUsers_ReturnsEmptyPage()
    {
        userSkills.GetUserIdsByFiltersAsync(
                Arg.Any<int?>(), Arg.Any<Guid?>(), Arg.Any<Guid?>(), Arg.Any<ProficiencyLevel?>(),
                Arg.Any<CancellationToken>())
            .Returns([]);

        var result = await CreateHandler()
            .Handle(new GetGuestListingsQuery(PageNumber: 1, PageSize: 10), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value.Items);
        Assert.Equal(0, result.Value.TotalCount);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(1, 0)]
    [InlineData(1, 51)]
    public async Task QueryValidator_RejectsOutOfRangePagination(int pageNumber, int pageSize)
    {
        var result = await new GetGuestListingsQueryValidator().ValidateAsync(
            new GetGuestListingsQuery(PageNumber: pageNumber, PageSize: pageSize));

        Assert.False(result.IsValid);
    }
}
