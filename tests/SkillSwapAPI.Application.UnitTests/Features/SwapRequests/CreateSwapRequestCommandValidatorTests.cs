using NSubstitute;
using SkillSwapAPI.Application.Features.SwapRequests.Commands.CreateSwapRequest;
using SkillSwapAPI.Application.UnitTests.Common;
using SkillSwapAPI.Domain.Modules.Users.Enums;
using Xunit;

namespace SkillSwapAPI.Application.UnitTests.Features.SwapRequests;

public class CreateSwapRequestCommandValidatorTests
{
    private readonly UnitOfWorkFixture _fixture = new();
    private readonly CreateSwapRequestCommandValidator _validator;

    private readonly Guid _requesterId = Guid.NewGuid();
    private readonly Guid _receiverId = Guid.NewGuid();
    private readonly Guid _offeredSkillId = Guid.NewGuid();
    private readonly Guid _requestedSkillId = Guid.NewGuid();

    public CreateSwapRequestCommandValidatorTests()
    {
        _validator = new CreateSwapRequestCommandValidator(_fixture.UnitOfWork);

        _fixture.Skills
            .ExistsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(true);

        _fixture.UserSkills
            .HasSkillAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<SkillType>(), Arg.Any<CancellationToken>())
            .Returns(true);

        _fixture.SwapRequests
            .HasPendingDuplicateAsync(
                Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(false);
    }

    private CreateSwapRequestCommand CreateCommand(
        Guid requesterId, Guid receiverId, Guid offeredSkillId, Guid requestedSkillId, string? schedule = "Weekends") =>
        new(requesterId, receiverId, offeredSkillId, requestedSkillId, schedule);

    [Fact]
    public async Task ValidateAsync_ShouldPass_WhenCommandIsValid()
    {
        var command = CreateCommand(_requesterId, _receiverId, _offeredSkillId, _requestedSkillId);

        var result = await _validator.ValidateAsync(command);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000000", "22222222-2222-2222-2222-222222222222", "33333333-3333-3333-3333-333333333333", "44444444-4444-4444-4444-444444444444")]
    [InlineData("11111111-1111-1111-1111-111111111111", "00000000-0000-0000-0000-000000000000", "33333333-3333-3333-3333-333333333333", "44444444-4444-4444-4444-444444444444")]
    [InlineData("11111111-1111-1111-1111-111111111111", "22222222-2222-2222-2222-222222222222", "00000000-0000-0000-0000-000000000000", "44444444-4444-4444-4444-444444444444")]
    [InlineData("11111111-1111-1111-1111-111111111111", "22222222-2222-2222-2222-222222222222", "33333333-3333-3333-3333-333333333333", "00000000-0000-0000-0000-000000000000")]
    public async Task ValidateAsync_ShouldFail_WhenAnyRequiredIdIsEmpty(
        string requesterId, string receiverId, string offeredSkillId, string requestedSkillId)
    {
        var command = CreateCommand(
            Guid.Parse(requesterId),
            Guid.Parse(receiverId),
            Guid.Parse(offeredSkillId),
            Guid.Parse(requestedSkillId));

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task ValidateAsync_ShouldFail_WhenReceiverIsSameAsRequester()
    {
        var command = CreateCommand(_requesterId, _requesterId, _offeredSkillId, _requestedSkillId);

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.ErrorMessage == "A user cannot send a swap request to themselves.");
    }

    [Fact]
    public async Task ValidateAsync_ShouldFail_WhenProposedScheduleDetailsExceedsMaxLength()
    {
        var command = CreateCommand(_requesterId, _receiverId, _offeredSkillId, _requestedSkillId, new string('a', 1001));

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateSwapRequestCommand.ProposedScheduleDetails));
    }

    [Fact]
    public async Task ValidateAsync_ShouldPass_WhenProposedScheduleDetailsIsExactlyMaxLength()
    {
        var command = CreateCommand(_requesterId, _receiverId, _offeredSkillId, _requestedSkillId, new string('a', 1000));

        var result = await _validator.ValidateAsync(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task ValidateAsync_ShouldFail_WhenOfferedSkillDoesNotExist()
    {
        var command = CreateCommand(_requesterId, _receiverId, _offeredSkillId, _requestedSkillId);
        _fixture.Skills
            .ExistsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.ErrorMessage == "The offered skill does not exist.");
        await _fixture.Skills.Received(1).ExistsAsync(_offeredSkillId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ValidateAsync_ShouldFail_WhenReceiverDoesNotOfferRequestedSkill()
    {
        var command = CreateCommand(_requesterId, _receiverId, _offeredSkillId, _requestedSkillId);
        _fixture.UserSkills
            .HasSkillAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<SkillType>(), Arg.Any<CancellationToken>())
            .Returns(false);

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.ErrorMessage == "The receiver does not offer the requested skill.");
        await _fixture.UserSkills.Received(1).HasSkillAsync(
            _receiverId, _requestedSkillId, SkillType.Offered, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ValidateAsync_ShouldFail_WhenPendingDuplicateExists()
    {
        var command = CreateCommand(_requesterId, _receiverId, _offeredSkillId, _requestedSkillId);
        _fixture.SwapRequests
            .HasPendingDuplicateAsync(
                Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "SwapRequest");
        Assert.Contains(result.Errors, error =>
            error.ErrorMessage == "A pending swap request to the same receiver for the same skills already exists.");
        await _fixture.SwapRequests.Received(1).HasPendingDuplicateAsync(
            _requesterId, _receiverId, _offeredSkillId, _requestedSkillId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ValidateAsync_ShouldNotCheckSkillExistence_WhenOfferedSkillIdIsEmpty()
    {
        var command = CreateCommand(_requesterId, _receiverId, Guid.Empty, _requestedSkillId);

        await _validator.ValidateAsync(command);

        await _fixture.Skills.DidNotReceive().ExistsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ValidateAsync_ShouldNotCheckDuplicate_WhenReceiverIdIsEmpty()
    {
        var command = CreateCommand(_requesterId, Guid.Empty, _offeredSkillId, _requestedSkillId);

        await _validator.ValidateAsync(command);

        await _fixture.SwapRequests.DidNotReceive().HasPendingDuplicateAsync(
            Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}
