using NSubstitute;
using SkillSwapAPI.Application.Features.Admin.Commands.CreateCategory;
using SkillSwapAPI.Application.Features.Admin.Commands.CreateSkill;
using SkillSwapAPI.Application.UnitTests.Common;
using SkillSwapAPI.Domain.Modules.Administration.Entities;
using SkillSwapAPI.Domain.Skills.Entities;
using Xunit;

namespace SkillSwapAPI.Application.UnitTests.Features.Admin;

public sealed class CatalogCommandHandlerTests
{
    [Fact]
    public async Task CreateCategory_PersistsActiveCategoryAndAuditRecord()
    {
        var fixture = new UnitOfWorkFixture();
        var adminId = Guid.NewGuid();
        SkillCategory? category = null;
        AuditLog? audit = null;
        fixture.SkillCategories.AddAsync(Arg.Do<SkillCategory>(value => category = value), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        fixture.AuditLogs.AddAsync(Arg.Do<AuditLog>(value => audit = value), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        var result = await new CreateCategoryCommandHandler(fixture.UnitOfWork)
            .Handle(new CreateCategoryCommand(adminId, "Languages", "Spoken languages"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Languages", category?.Name);
        Assert.True(category?.IsActive);
        Assert.Equal("CategoryCreated", audit?.Action);
        Assert.Equal(adminId, audit?.AdminId);
        await fixture.UnitOfWork.Received(2).CompleteAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateSkill_WhenCategoryDoesNotExist_ReturnsErrorWithoutCreatingOrAuditing()
    {
        var fixture = new UnitOfWorkFixture();
        fixture.SkillCategories.ExistsAsync(55, Arg.Any<CancellationToken>()).Returns(false);

        var result = await new CreateSkillCommandHandler(fixture.UnitOfWork)
            .Handle(new CreateSkillCommand(Guid.NewGuid(), 55, "Rust", null), CancellationToken.None);

        Assert.False(result.IsSuccess);
        await fixture.Skills.DidNotReceive().AddAsync(Arg.Any<Skill>(), Arg.Any<CancellationToken>());
        await fixture.AuditLogs.DidNotReceive().AddAsync(Arg.Any<AuditLog>(), Arg.Any<CancellationToken>());
        await fixture.UnitOfWork.DidNotReceive().CompleteAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateSkill_WhenCategoryExists_CreatesSkillAndAuditEntry()
    {
        var fixture = new UnitOfWorkFixture();
        var adminId = Guid.NewGuid();
        fixture.SkillCategories.ExistsAsync(1, Arg.Any<CancellationToken>()).Returns(true);
        Skill? created = null;
        AuditLog? audit = null;
        fixture.Skills.AddAsync(Arg.Do<Skill>(value => created = value), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        fixture.AuditLogs.AddAsync(Arg.Do<AuditLog>(value => audit = value), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        var result = await new CreateSkillCommandHandler(fixture.UnitOfWork)
            .Handle(new CreateSkillCommand(adminId, 1, "Rust", "Systems language"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Rust", created?.Name);
        Assert.Equal(1, created?.CategoryId);
        Assert.Equal("SkillCreated", audit?.Action);
        Assert.Equal(created?.Id.ToString(), audit?.TargetEntityId);
        await fixture.UnitOfWork.Received(1).CompleteAsync(Arg.Any<CancellationToken>());
    }
}
