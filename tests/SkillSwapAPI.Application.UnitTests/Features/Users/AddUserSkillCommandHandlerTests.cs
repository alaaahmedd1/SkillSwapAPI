using NSubstitute;
using SkillSwapAPI.Application.Common.Errors;
using SkillSwapAPI.Application.Features.Users.Commands.AddUserSkill;
using SkillSwapAPI.Application.UnitTests.Common;
using SkillSwapAPI.Domain.Common.Results;
using SkillSwapAPI.Domain.Modules.Users.Entities;
using SkillSwapAPI.Domain.Modules.Users.Enums;
using SkillSwapAPI.Domain.Skills.Entities;
using Xunit;

namespace SkillSwapAPI.Application.UnitTests.Features.Users;

public class AddUserSkillCommandHandlerTests
{
    private readonly UnitOfWorkFixture _fixture = new();

    private AddUserSkillCommandHandler CreateHandler() => new(_fixture.UnitOfWork);

    private static AddUserSkillCommand CreateCommand(
        Guid? userId = null,
        Guid? skillId = null,
        SkillType type = SkillType.Offered,
        ProficiencyLevel level = ProficiencyLevel.Expert,
        int? yearsOfExperience = 5) =>
        new(userId ?? TestData.UserId, skillId ?? Guid.NewGuid(), type, level, yearsOfExperience);

    [Fact]
    public async Task Handle_ShouldReturnSkillNotFound_WhenSkillDoesNotExist()
    {
        var command = CreateCommand();
        _fixture.Skills
            .GetWithCategoryAsync(command.SkillId, Arg.Any<CancellationToken>())
            .Returns((Skill?)null);

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal("Skills.NotFound", result.TopError.Code);
        Assert.Equal(ErrorKind.Validation, result.TopError.Type);
        await _fixture.UserSkills.DidNotReceive()
            .HasSkillWithDifferentTypeAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<SkillType>(), Arg.Any<CancellationToken>());
        await _fixture.UserSkills.DidNotReceive().AddAsync(Arg.Any<UserSkill>(), Arg.Any<CancellationToken>());
        await _fixture.UnitOfWork.DidNotReceive().CompleteAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnDuplicateSkillType_WhenUserAlreadyHasSkillWithDifferentType()
    {
        var skill = TestData.Skill(name: "C#", categoryId: 1, category: TestData.Category(1, "Programming"));
        var command = CreateCommand(skillId: skill.Id);
        _fixture.Skills
            .GetWithCategoryAsync(skill.Id, Arg.Any<CancellationToken>())
            .Returns(skill);
        _fixture.UserSkills
            .HasSkillWithDifferentTypeAsync(command.UserId, skill.Id, command.Type, Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal("Skills.DuplicateType", result.TopError.Code);
        Assert.Equal(ErrorKind.Validation, result.TopError.Type);
        await _fixture.UserSkills.DidNotReceive().AddAsync(Arg.Any<UserSkill>(), Arg.Any<CancellationToken>());
        await _fixture.UnitOfWork.DidNotReceive().CompleteAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldAddUserSkillAndReturnDto_WhenCommandIsValid()
    {
        var skill = TestData.Skill(name: "C#", categoryId: 1, category: TestData.Category(1, "Programming"));
        var command = CreateCommand(skillId: skill.Id, type: SkillType.Offered, level: ProficiencyLevel.Expert, yearsOfExperience: 5);
        _fixture.Skills
            .GetWithCategoryAsync(skill.Id, Arg.Any<CancellationToken>())
            .Returns(skill);
        _fixture.UserSkills
            .HasSkillWithDifferentTypeAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<SkillType>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.False(result.IsError);
        Assert.Equal(skill.Id, result.Value.SkillId);
        Assert.Equal("C#", result.Value.SkillName);
        Assert.Equal("Programming", result.Value.CategoryName);
        Assert.Equal(SkillType.Offered, result.Value.Type);
        Assert.Equal(ProficiencyLevel.Expert, result.Value.ProficiencyLevel);
        Assert.Equal(5, result.Value.YearsOfExperience);
        Assert.NotEqual(Guid.Empty, result.Value.Id);

        await _fixture.Skills.Received(1).GetWithCategoryAsync(skill.Id, Arg.Any<CancellationToken>());
        await _fixture.UserSkills.Received(1)
            .HasSkillWithDifferentTypeAsync(command.UserId, skill.Id, SkillType.Offered, Arg.Any<CancellationToken>());
        await _fixture.UserSkills.Received(1).AddAsync(
            Arg.Is<UserSkill>(userSkill =>
                userSkill.Id == result.Value.Id
                && userSkill.UserId == command.UserId
                && userSkill.SkillId == skill.Id
                && userSkill.Type == SkillType.Offered
                && userSkill.ProficiencyLevel == ProficiencyLevel.Expert
                && userSkill.YearsOfExperience == 5),
            Arg.Any<CancellationToken>());
        await _fixture.UnitOfWork.Received(1).CompleteAsync(Arg.Any<CancellationToken>());
    }
}

public class AddUserSkillCommandValidatorTests
{
    private readonly AddUserSkillCommandValidator _validator = new();

    [Fact]
    public void Validate_ShouldPass_WhenCommandIsValid()
    {
        var command = new AddUserSkillCommand(
            Guid.NewGuid(), Guid.NewGuid(), SkillType.Offered, ProficiencyLevel.Intermediate, 3);

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void Validate_ShouldFail_WhenUserIdOrSkillIdIsEmpty(bool emptyUserId, bool emptySkillId)
    {
        var command = new AddUserSkillCommand(
            emptyUserId ? Guid.Empty : Guid.NewGuid(),
            emptySkillId ? Guid.Empty : Guid.NewGuid(),
            SkillType.Offered, ProficiencyLevel.Intermediate, 3);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData(99, 2)]
    [InlineData(1, 99)]
    [InlineData(0, 2)]
    [InlineData(1, 0)]
    public void Validate_ShouldFail_WhenTypeOrProficiencyLevelIsNotDefined(int type, int level)
    {
        var command = new AddUserSkillCommand(
            Guid.NewGuid(), Guid.NewGuid(), (SkillType)type, (ProficiencyLevel)level, 3);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-10)]
    public void Validate_ShouldFail_WhenYearsOfExperienceIsNegative(int yearsOfExperience)
    {
        var command = new AddUserSkillCommand(
            Guid.NewGuid(), Guid.NewGuid(), SkillType.Offered, ProficiencyLevel.Intermediate, yearsOfExperience);

        var result = _validator.Validate(command);

        Assert.False(result.IsValid);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(25)]
    public void Validate_ShouldPass_WhenYearsOfExperienceIsNullOrNonNegative(int? yearsOfExperience)
    {
        var command = new AddUserSkillCommand(
            Guid.NewGuid(), Guid.NewGuid(), SkillType.Offered, ProficiencyLevel.Intermediate, yearsOfExperience);

        var result = _validator.Validate(command);

        Assert.True(result.IsValid);
    }
}
