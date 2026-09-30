using NSubstitute;
using SkillSwapAPI.Application.Features.Users.Commands.RemoveUserSkill;
using SkillSwapAPI.Application.UnitTests.Common;
using Xunit;

namespace SkillSwapAPI.Application.UnitTests.Features.Users;

public sealed class RemoveUserSkillCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenSkillIsNotOwnedByCaller_ReturnsNotFoundAndDoesNotDelete()
    {
        var fixture = new UnitOfWorkFixture();
        var skillId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        fixture.UserSkills.GetByIdAndUserAsync(skillId, userId, Arg.Any<CancellationToken>()).Returns((SkillSwapAPI.Domain.Modules.Users.Entities.UserSkill?)null);

        var result = await new RemoveUserSkillCommandHandler(fixture.UnitOfWork)
            .Handle(new RemoveUserSkillCommand(userId, skillId), CancellationToken.None);

        Assert.False(result.IsSuccess);
        fixture.UserSkills.DidNotReceive().Delete(Arg.Any<SkillSwapAPI.Domain.Modules.Users.Entities.UserSkill>());
        await fixture.UnitOfWork.DidNotReceive().CompleteAsync(Arg.Any<CancellationToken>());
    }
}
