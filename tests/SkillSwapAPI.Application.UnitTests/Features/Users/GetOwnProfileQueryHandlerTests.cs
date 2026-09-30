using System.Security.Claims;
using NSubstitute;
using SkillSwapAPI.Application.Common.Interfaces.Identity;
using SkillSwapAPI.Application.Features.Identity.Dtos;
using SkillSwapAPI.Application.Features.Users.Queries.GetOwnProfile;
using SkillSwapAPI.Application.UnitTests.Common;
using SkillSwapAPI.Domain.Common.Results;
using SkillSwapAPI.Domain.Modules.Users.Enums;
using Xunit;

namespace SkillSwapAPI.Application.UnitTests.Features.Users;

public sealed class GetOwnProfileQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsIdentityAndOwnedSkillDetailsForRequestedUser()
    {
        var fixture = new UnitOfWorkFixture();
        var identity = Substitute.For<IIdentityService>();
        var userId = Guid.NewGuid();
        var user = new AppUserDto(userId, "ada@example.com", "Ada", "Lovelace", 4.8m, 5, true,
            DateTimeOffset.UtcNow, new List<string>(), new List<Claim>());
        identity.GetUserByIdAsync(userId.ToString(), Arg.Any<CancellationToken>()).Returns(Result<AppUserDto>.FromSuccess(user));
        var skill = TestData.UserSkill(userId: userId, skillId: Guid.NewGuid(), type: SkillType.Offered, level: ProficiencyLevel.Expert);
        fixture.UserSkills.GetByUserAsync(userId, Arg.Any<CancellationToken>()).Returns([skill]);

        var result = await new GetOwnProfileQueryHandler(fixture.UnitOfWork, identity)
            .Handle(new GetOwnProfileQuery(userId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(userId, result.Value.UserId);
        Assert.Equal("ada@example.com", result.Value.Email);
        Assert.Equal(4.8m, result.Value.AverageRating);
        Assert.Single(result.Value.UserSkills);
        Assert.Equal(skill.Skill.Name, result.Value.UserSkills[0].SkillName);
        Assert.Equal(SkillType.Offered, result.Value.UserSkills[0].Type);
    }
}
