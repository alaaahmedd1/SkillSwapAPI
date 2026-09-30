using NSubstitute;
using SkillSwapAPI.Application.Features.Reviews.Commands.SubmitReview;
using SkillSwapAPI.Application.UnitTests.Common;
using SkillSwapAPI.Domain.Modules.Badges.Entities;
using Xunit;

namespace SkillSwapAPI.Application.UnitTests.Features.Reviews;

public class SubmitReviewCommandValidatorTests
{
    private readonly UnitOfWorkFixture _fixture = new();
    private readonly SubmitReviewCommandValidator _validator;

    public SubmitReviewCommandValidatorTests()
    {
        _validator = new SubmitReviewCommandValidator(_fixture.UnitOfWork);
    }

    private SubmitReviewCommand CreateValidCommand() => new(
        Guid.NewGuid(), TestData.UserId, TestData.OtherUserId, 4, "Great session", null);

    private void SetupExistingReview(bool exists)
    {
        _fixture.Reviews
            .HasReviewAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(exists));
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenCommandIsValid()
    {
        SetupExistingReview(false);

        var result = await _validator.ValidateAsync(CreateValidCommand());

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenCommentIsNull()
    {
        SetupExistingReview(false);

        var result = await _validator.ValidateAsync(CreateValidCommand() with { Comment = null });

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(6)]
    [InlineData(100)]
    public async Task Validate_ShouldFail_WhenRatingIsOutsideRange(int rating)
    {
        SetupExistingReview(false);

        var result = await _validator.ValidateAsync(CreateValidCommand() with { Rating = rating });

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenRatingIsAtBoundaries()
    {
        SetupExistingReview(false);

        var lowestResult = await _validator.ValidateAsync(CreateValidCommand() with { Rating = 1 });
        var highestResult = await _validator.ValidateAsync(CreateValidCommand() with { Rating = 5 });

        Assert.True(lowestResult.IsValid);
        Assert.True(highestResult.IsValid);
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenCommentExceedsMaxLength()
    {
        SetupExistingReview(false);

        var result = await _validator.ValidateAsync(CreateValidCommand() with { Comment = new string('a', 1001) });

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenCommentIsExactlyMaxLength()
    {
        SetupExistingReview(false);

        var result = await _validator.ValidateAsync(CreateValidCommand() with { Comment = new string('a', 1000) });

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenSwapRequestIdIsEmpty()
    {
        var result = await _validator.ValidateAsync(CreateValidCommand() with { SwapRequestId = Guid.Empty });

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenReviewerIdIsEmpty()
    {
        var result = await _validator.ValidateAsync(CreateValidCommand() with { ReviewerId = Guid.Empty });

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenRevieweeIdIsEmpty()
    {
        SetupExistingReview(false);

        var result = await _validator.ValidateAsync(CreateValidCommand() with { RevieweeId = Guid.Empty });

        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenReviewAlreadyExistsForSwap()
    {
        SetupExistingReview(true);

        var result = await _validator.ValidateAsync(CreateValidCommand());

        Assert.False(result.IsValid);
        var error = Assert.Single(result.Errors);
        Assert.Equal("You have already submitted a review for this swap request.", error.ErrorMessage);
        Assert.Equal("Review", error.PropertyName);
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenNoReviewExistsForSwap()
    {
        SetupExistingReview(false);
        var command = CreateValidCommand();

        var result = await _validator.ValidateAsync(command);

        Assert.True(result.IsValid);
        await _fixture.Reviews.Received(1).HasReviewAsync(
            command.SwapRequestId, command.ReviewerId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenBadgeDoesNotExist()
    {
        SetupExistingReview(false);
        _fixture.Badges
            .GetByIdAsync(9, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Badge?>(null));

        var result = await _validator.ValidateAsync(CreateValidCommand() with { BadgeId = 9 });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error =>
            error.ErrorMessage == "The selected badge was not found or is no longer available.");
    }

    [Fact]
    public async Task Validate_ShouldFail_WhenBadgeIsInactive()
    {
        SetupExistingReview(false);
        _fixture.Badges
            .GetByIdAsync(9, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Badge?>(TestData.Badge(id: 9, name: "Retired", isActive: false)));

        var result = await _validator.ValidateAsync(CreateValidCommand() with { BadgeId = 9 });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error =>
            error.ErrorMessage == "The selected badge was not found or is no longer available.");
    }

    [Fact]
    public async Task Validate_ShouldPass_WhenBadgeExistsAndIsActive()
    {
        SetupExistingReview(false);
        _fixture.Badges
            .GetByIdAsync(9, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Badge?>(TestData.Badge(id: 9, name: "Super Patient")));

        var result = await _validator.ValidateAsync(CreateValidCommand() with { BadgeId = 9 });

        Assert.True(result.IsValid);
        await _fixture.Badges.Received(1).GetByIdAsync(9, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Validate_ShouldSkipBadgeRule_WhenBadgeIdIsNull()
    {
        SetupExistingReview(false);

        var result = await _validator.ValidateAsync(CreateValidCommand() with { BadgeId = null });

        Assert.True(result.IsValid);
        await _fixture.Badges.DidNotReceive().GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Validate_ShouldSkipDuplicateRule_WhenSwapRequestIdIsEmpty()
    {
        var result = await _validator.ValidateAsync(CreateValidCommand() with { SwapRequestId = Guid.Empty });

        Assert.False(result.IsValid);
        await _fixture.Reviews.DidNotReceive().HasReviewAsync(
            Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}
