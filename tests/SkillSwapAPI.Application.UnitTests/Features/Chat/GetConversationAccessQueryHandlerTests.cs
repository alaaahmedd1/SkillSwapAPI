using NSubstitute;
using SkillSwapAPI.Application.Features.Chat.Queries.GetConversationAccess;
using SkillSwapAPI.Application.UnitTests.Common;
using SkillSwapAPI.Domain.Common.Results;
using SkillSwapAPI.Domain.Modules.SwapRequests.Entities;
using SkillSwapAPI.Domain.Modules.SwapRequests.Enums;
using Xunit;

namespace SkillSwapAPI.Application.UnitTests.Features.Chat;

public class GetConversationAccessQueryHandlerTests
{
    private readonly UnitOfWorkFixture _fixture = new();

    [Fact]
    public async Task Handle_ShouldReturnConversationNotFound_WhenSwapRequestIsMissing()
    {
        var query = new GetConversationAccessQuery(Guid.NewGuid(), TestData.UserId);
        _fixture.Conversations
            .GetSwapRequestByConversationAsync(query.ConversationId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<SwapRequest?>(null));

        var handler = new GetConversationAccessQueryHandler(_fixture.UnitOfWork);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal("Chat.ConversationNotFound", result.TopError.Code);
        Assert.Equal(ErrorKind.NotFound, result.TopError.Type);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotSwapParticipant_WhenUserIsNeitherRequesterNorReceiver()
    {
        var outsider = Guid.NewGuid();
        var query = new GetConversationAccessQuery(Guid.NewGuid(), outsider);
        var swapRequest = TestData.SwapRequest(
            requesterId: TestData.UserId,
            receiverId: TestData.OtherUserId,
            status: SwapRequestStatus.Accepted);
        _fixture.Conversations
            .GetSwapRequestByConversationAsync(query.ConversationId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<SwapRequest?>(swapRequest));

        var handler = new GetConversationAccessQueryHandler(_fixture.UnitOfWork);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal("Chat.NotSwapParticipant", result.TopError.Code);
        Assert.Equal(ErrorKind.Forbidden, result.TopError.Type);
    }

    [Theory]
    [InlineData(SwapRequestStatus.Pending)]
    [InlineData(SwapRequestStatus.Rejected)]
    [InlineData(SwapRequestStatus.Cancelled)]
    public async Task Handle_ShouldReturnSwapNotActive_WhenSwapStatusIsNotAcceptedOrCompleted(SwapRequestStatus status)
    {
        var query = new GetConversationAccessQuery(Guid.NewGuid(), TestData.UserId);
        var swapRequest = TestData.SwapRequest(
            requesterId: TestData.UserId,
            receiverId: TestData.OtherUserId,
            status: status);
        _fixture.Conversations
            .GetSwapRequestByConversationAsync(query.ConversationId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<SwapRequest?>(swapRequest));

        var handler = new GetConversationAccessQueryHandler(_fixture.UnitOfWork);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal("Chat.SwapNotActive", result.TopError.Code);
        Assert.Equal(ErrorKind.Forbidden, result.TopError.Type);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenUserIsRequesterOfAcceptedSwap()
    {
        var query = new GetConversationAccessQuery(Guid.NewGuid(), TestData.UserId);
        var swapRequest = TestData.SwapRequest(
            requesterId: TestData.UserId,
            receiverId: TestData.OtherUserId,
            status: SwapRequestStatus.Accepted);
        _fixture.Conversations
            .GetSwapRequestByConversationAsync(query.ConversationId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<SwapRequest?>(swapRequest));

        var handler = new GetConversationAccessQueryHandler(_fixture.UnitOfWork);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.False(result.IsError);
        Assert.True(result.IsSuccess);
        Assert.Equal(default(Success), result.Value);
        await _fixture.Conversations.Received(1).GetSwapRequestByConversationAsync(
            query.ConversationId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenUserIsReceiverOfCompletedSwap()
    {
        var query = new GetConversationAccessQuery(Guid.NewGuid(), TestData.OtherUserId);
        var swapRequest = TestData.SwapRequest(
            requesterId: TestData.UserId,
            receiverId: TestData.OtherUserId,
            status: SwapRequestStatus.Completed);
        _fixture.Conversations
            .GetSwapRequestByConversationAsync(query.ConversationId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<SwapRequest?>(swapRequest));

        var handler = new GetConversationAccessQueryHandler(_fixture.UnitOfWork);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.False(result.IsError);
        Assert.Equal(default(Success), result.Value);
    }
}
