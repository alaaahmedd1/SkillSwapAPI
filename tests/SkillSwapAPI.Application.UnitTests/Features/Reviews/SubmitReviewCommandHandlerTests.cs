using System.Linq.Expressions;
using MediatR;
using NSubstitute;
using SkillSwapAPI.Application.Common.Interfaces.Identity;
using SkillSwapAPI.Application.Features.Reviews.Commands.SubmitReview;
using SkillSwapAPI.Application.Features.Users.Dtos;
using SkillSwapAPI.Application.UnitTests.Common;
using SkillSwapAPI.Domain.Common.Results;
using SkillSwapAPI.Domain.Modules.Badges.Entities;
using SkillSwapAPI.Domain.Modules.Reviews.Entities;
using SkillSwapAPI.Domain.Modules.Reviews.Events;
using SkillSwapAPI.Domain.Modules.SwapRequests.Entities;
using SkillSwapAPI.Domain.Modules.SwapRequests.Enums;
using Xunit;

namespace SkillSwapAPI.Application.UnitTests.Features.Reviews;

public class SubmitReviewCommandHandlerTests
{
    private readonly UnitOfWorkFixture _fixture = new();
    private readonly IIdentityService _identityService = Substitute.For<IIdentityService>();
    private readonly IPublisher _publisher = Substitute.For<IPublisher>();

    private SubmitReviewCommandHandler CreateHandler() => new(_fixture.UnitOfWork, _identityService, _publisher);

    private void SetupSwapRequest(SwapRequest swapRequest)
    {
        _fixture.SwapRequests
            .FindAsync(Arg.Any<Expression<Func<SwapRequest, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<SwapRequest?>(swapRequest));
    }

    private void SetupReviewerProfile(ProfileIdentityDto? profile)
    {
        var profiles = profile is null
            ? new List<ProfileIdentityDto>()
            : new List<ProfileIdentityDto> { profile };
        _identityService
            .GetProfilesAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<ProfileIdentityDto>>(profiles));
    }

    [Fact]
    public async Task Handle_ShouldReturnSwapNotFound_WhenSwapRequestDoesNotExist()
    {
        var command = new SubmitReviewCommand(Guid.NewGuid(), TestData.UserId, TestData.OtherUserId, 5, "Great", null);
        SetupSwapRequest(null!);

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal("SwapRequests.NotFound", result.TopError.Code);
        Assert.Equal(ErrorKind.NotFound, result.TopError.Type);
        await _fixture.Reviews.DidNotReceive().AddAsync(Arg.Any<Review>(), Arg.Any<CancellationToken>());
        await _fixture.UnitOfWork.DidNotReceive().CompleteAsync(Arg.Any<CancellationToken>());
        await _publisher.DidNotReceive().Publish(Arg.Any<ReviewSubmittedEvent>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(SwapRequestStatus.Pending)]
    [InlineData(SwapRequestStatus.Accepted)]
    [InlineData(SwapRequestStatus.Rejected)]
    [InlineData(SwapRequestStatus.Cancelled)]
    public async Task Handle_ShouldReturnSwapNotCompleted_WhenSwapStatusIsNotCompleted(SwapRequestStatus status)
    {
        var command = new SubmitReviewCommand(Guid.NewGuid(), TestData.UserId, TestData.OtherUserId, 5, "Great", null);
        SetupSwapRequest(TestData.SwapRequest(
            requesterId: TestData.UserId,
            receiverId: TestData.OtherUserId,
            status: status));

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal("Reviews.SwapNotCompleted", result.TopError.Code);
        Assert.Equal(ErrorKind.Validation, result.TopError.Type);
        await _fixture.Reviews.DidNotReceive().AddAsync(Arg.Any<Review>(), Arg.Any<CancellationToken>());
        await _fixture.UnitOfWork.DidNotReceive().CompleteAsync(Arg.Any<CancellationToken>());
        await _publisher.DidNotReceive().Publish(Arg.Any<ReviewSubmittedEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnNotParticipant_WhenReviewerIsNeitherRequesterNorReceiver()
    {
        var outsider = Guid.NewGuid();
        var command = new SubmitReviewCommand(Guid.NewGuid(), outsider, TestData.OtherUserId, 5, "Great", null);
        SetupSwapRequest(TestData.SwapRequest(
            requesterId: TestData.UserId,
            receiverId: TestData.OtherUserId,
            status: SwapRequestStatus.Completed));

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal("Reviews.NotParticipant", result.TopError.Code);
        Assert.Equal(ErrorKind.Forbidden, result.TopError.Type);
        await _fixture.Reviews.DidNotReceive().AddAsync(Arg.Any<Review>(), Arg.Any<CancellationToken>());
        await _fixture.UnitOfWork.DidNotReceive().CompleteAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnInvalidReviewee_WhenRequesterReviewsSomeoneOtherThanReceiver()
    {
        var stranger = Guid.NewGuid();
        var command = new SubmitReviewCommand(Guid.NewGuid(), TestData.UserId, stranger, 5, "Great", null);
        SetupSwapRequest(TestData.SwapRequest(
            requesterId: TestData.UserId,
            receiverId: TestData.OtherUserId,
            status: SwapRequestStatus.Completed));

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal("Reviews.InvalidReviewee", result.TopError.Code);
        Assert.Equal(ErrorKind.Validation, result.TopError.Type);
        await _fixture.Reviews.DidNotReceive().AddAsync(Arg.Any<Review>(), Arg.Any<CancellationToken>());
        await _fixture.UnitOfWork.DidNotReceive().CompleteAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnInvalidReviewee_WhenReceiverReviewsSomeoneOtherThanRequester()
    {
        var stranger = Guid.NewGuid();
        var command = new SubmitReviewCommand(Guid.NewGuid(), TestData.OtherUserId, stranger, 4, "Good", null);
        SetupSwapRequest(TestData.SwapRequest(
            requesterId: TestData.UserId,
            receiverId: TestData.OtherUserId,
            status: SwapRequestStatus.Completed));

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal("Reviews.InvalidReviewee", result.TopError.Code);
        Assert.Equal(ErrorKind.Validation, result.TopError.Type);
    }

    [Fact]
    public async Task Handle_ShouldReturnInvalidReviewee_WhenReviewerReviewsThemselves()
    {
        var command = new SubmitReviewCommand(Guid.NewGuid(), TestData.UserId, TestData.UserId, 5, "Great", null);
        SetupSwapRequest(TestData.SwapRequest(
            requesterId: TestData.UserId,
            receiverId: TestData.OtherUserId,
            status: SwapRequestStatus.Completed));

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal("Reviews.InvalidReviewee", result.TopError.Code);
        Assert.Equal(ErrorKind.Validation, result.TopError.Type);
    }

    [Fact]
    public async Task Handle_ShouldReturnBadgeNotFound_WhenBadgeDoesNotExist()
    {
        var command = new SubmitReviewCommand(Guid.NewGuid(), TestData.UserId, TestData.OtherUserId, 5, "Great", 7);
        SetupSwapRequest(TestData.SwapRequest(
            requesterId: TestData.UserId,
            receiverId: TestData.OtherUserId,
            status: SwapRequestStatus.Completed));
        _fixture.Badges
            .GetByIdAsync(7, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Badge?>(null));

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal("Badges.BadgeNotFound", result.TopError.Code);
        Assert.Equal(ErrorKind.NotFound, result.TopError.Type);
        await _fixture.Reviews.DidNotReceive().AddAsync(Arg.Any<Review>(), Arg.Any<CancellationToken>());
        await _fixture.UserBadgeAwards.DidNotReceive().AddAsync(Arg.Any<UserBadgeAward>(), Arg.Any<CancellationToken>());
        await _fixture.UnitOfWork.DidNotReceive().CompleteAsync(Arg.Any<CancellationToken>());
        await _publisher.DidNotReceive().Publish(Arg.Any<ReviewSubmittedEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnBadgeNotFound_WhenBadgeIsInactive()
    {
        var command = new SubmitReviewCommand(Guid.NewGuid(), TestData.UserId, TestData.OtherUserId, 5, "Great", 7);
        SetupSwapRequest(TestData.SwapRequest(
            requesterId: TestData.UserId,
            receiverId: TestData.OtherUserId,
            status: SwapRequestStatus.Completed));
        _fixture.Badges
            .GetByIdAsync(7, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Badge?>(TestData.Badge(id: 7, name: "Retired", isActive: false)));

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal("Badges.BadgeNotFound", result.TopError.Code);
        Assert.Equal(ErrorKind.NotFound, result.TopError.Type);
        await _fixture.Reviews.DidNotReceive().AddAsync(Arg.Any<Review>(), Arg.Any<CancellationToken>());
        await _fixture.UserBadgeAwards.DidNotReceive().AddAsync(Arg.Any<UserBadgeAward>(), Arg.Any<CancellationToken>());
        await _fixture.UnitOfWork.DidNotReceive().CompleteAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldAddReviewPublishEventAndReturnDto_WhenNoBadgeIsAwarded()
    {
        var swapRequest = TestData.SwapRequest(
            requesterId: TestData.UserId,
            receiverId: TestData.OtherUserId,
            status: SwapRequestStatus.Completed);
        SetupSwapRequest(swapRequest);
        var command = new SubmitReviewCommand(swapRequest.Id, TestData.UserId, TestData.OtherUserId, 5, "Great session", null);

        Review? capturedReview = null;
        _fixture.Reviews
            .AddAsync(Arg.Do<Review>(review => capturedReview = review), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        SetupReviewerProfile(TestData.Profile(userId: TestData.UserId, firstName: "Ada", lastName: "Lovelace"));
        _publisher
            .Publish(Arg.Any<ReviewSubmittedEvent>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.False(result.IsError);
        Assert.NotNull(capturedReview);
        Assert.Equal(command.SwapRequestId, capturedReview!.SwapRequestId);
        Assert.Equal(command.ReviewerId, capturedReview.ReviewerId);
        Assert.Equal(command.RevieweeId, capturedReview.RevieweeId);
        Assert.Equal(command.Rating, capturedReview.Rating);
        Assert.Equal(command.Comment, capturedReview.Comment);

        Assert.Equal(capturedReview.Id, result.Value.Id);
        Assert.Equal(command.SwapRequestId, result.Value.SwapRequestId);
        Assert.Equal(command.ReviewerId, result.Value.ReviewerId);
        Assert.Equal("Ada", result.Value.ReviewerFirstName);
        Assert.Equal("Lovelace", result.Value.ReviewerLastName);
        Assert.Equal(command.RevieweeId, result.Value.RevieweeId);
        Assert.Equal(5, result.Value.Rating);
        Assert.Equal("Great session", result.Value.Comment);
        Assert.Equal(capturedReview.CreatedAtUtc, result.Value.CreatedAtUtc);

        await _fixture.UserBadgeAwards.DidNotReceive().AddAsync(Arg.Any<UserBadgeAward>(), Arg.Any<CancellationToken>());
        await _fixture.UnitOfWork.Received(1).CompleteAsync(Arg.Any<CancellationToken>());
        await _publisher.Received(1).Publish(
            Arg.Is<ReviewSubmittedEvent>(domainEvent =>
                domainEvent.ReviewId == capturedReview.Id &&
                domainEvent.SwapRequestId == command.SwapRequestId &&
                domainEvent.ReviewerId == command.ReviewerId &&
                domainEvent.RevieweeId == command.RevieweeId &&
                domainEvent.Rating == command.Rating &&
                domainEvent.CreatedAtUtc == capturedReview.CreatedAtUtc),
            Arg.Any<CancellationToken>());
        await _identityService.Received(1).GetProfilesAsync(
            Arg.Is<IReadOnlyCollection<Guid>>(ids => ids.Count == 1 && ids.Contains(command.ReviewerId)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldAddReviewAndBadgeAwardInSameUnitOfWork_WhenBadgeIsValid()
    {
        var command = new SubmitReviewCommand(Guid.NewGuid(), TestData.UserId, TestData.OtherUserId, 4, "Helpful", 3);
        SetupSwapRequest(TestData.SwapRequest(
            requesterId: TestData.UserId,
            receiverId: TestData.OtherUserId,
            status: SwapRequestStatus.Completed));
        _fixture.Badges
            .GetByIdAsync(3, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Badge?>(TestData.Badge(id: 3, name: "Super Patient")));

        Review? capturedReview = null;
        _fixture.Reviews
            .AddAsync(Arg.Do<Review>(review => capturedReview = review), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        UserBadgeAward? capturedAward = null;
        _fixture.UserBadgeAwards
            .AddAsync(Arg.Do<UserBadgeAward>(award => capturedAward = award), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        SetupReviewerProfile(TestData.Profile(userId: TestData.UserId, firstName: "Ada", lastName: "Lovelace"));
        _publisher
            .Publish(Arg.Any<ReviewSubmittedEvent>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.False(result.IsError);
        Assert.Equal(4, result.Value.Rating);
        Assert.Equal("Helpful", result.Value.Comment);

        Assert.NotNull(capturedReview);
        Assert.NotNull(capturedAward);
        Assert.Equal(capturedAward!.ReviewId, capturedReview!.Id);
        Assert.Equal(3, capturedAward.BadgeId);
        Assert.Equal(command.ReviewerId, capturedAward.ReviewerId);
        Assert.Equal(command.RevieweeId, capturedAward.RevieweeId);
        Assert.NotEqual(Guid.Empty, capturedAward.Id);

        await _fixture.Reviews.Received(1).AddAsync(Arg.Any<Review>(), Arg.Any<CancellationToken>());
        await _fixture.UserBadgeAwards.Received(1).AddAsync(Arg.Any<UserBadgeAward>(), Arg.Any<CancellationToken>());
        await _fixture.UnitOfWork.Received(1).CompleteAsync(Arg.Any<CancellationToken>());
        await _publisher.Received(1).Publish(
            Arg.Any<ReviewSubmittedEvent>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyReviewerNames_WhenReviewerProfileIsNotFound()
    {
        var command = new SubmitReviewCommand(Guid.NewGuid(), TestData.UserId, TestData.OtherUserId, 5, null, null);
        SetupSwapRequest(TestData.SwapRequest(
            requesterId: TestData.UserId,
            receiverId: TestData.OtherUserId,
            status: SwapRequestStatus.Completed));
        _fixture.Reviews
            .AddAsync(Arg.Any<Review>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        SetupReviewerProfile(null);
        _publisher
            .Publish(Arg.Any<ReviewSubmittedEvent>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.False(result.IsError);
        Assert.Equal(string.Empty, result.Value.ReviewerFirstName);
        Assert.Equal(string.Empty, result.Value.ReviewerLastName);
        Assert.Null(result.Value.Comment);
    }
}
