using NSubstitute;
using SkillSwapAPI.Application.Features.Chat.Queries.GetChatHistory;
using SkillSwapAPI.Application.UnitTests.Common;
using SkillSwapAPI.Domain.Common.Results;
using SkillSwapAPI.Domain.Modules.Chat.Entities;
using SkillSwapAPI.Domain.Modules.SwapRequests.Entities;
using SkillSwapAPI.Domain.Modules.SwapRequests.Enums;
using Xunit;

namespace SkillSwapAPI.Application.UnitTests.Features.Chat;

public class GetChatHistoryQueryHandlerTests
{
    private readonly UnitOfWorkFixture _fixture = new();

    [Fact]
    public async Task Handle_ShouldReturnConversationNotFound_WhenSwapRequestIsMissing()
    {
        var query = new GetChatHistoryQuery(Guid.NewGuid(), TestData.UserId);
        _fixture.Conversations
            .GetSwapRequestByConversationAsync(query.ConversationId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<SwapRequest?>(null));

        var handler = new GetChatHistoryQueryHandler(_fixture.UnitOfWork);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal("Chat.ConversationNotFound", result.TopError.Code);
        Assert.Equal(ErrorKind.NotFound, result.TopError.Type);
        await _fixture.Messages.DidNotReceive().GetPagedByConversationAsync(
            Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnNotSwapParticipant_WhenUserIsNeitherRequesterNorReceiver()
    {
        var outsider = Guid.NewGuid();
        var query = new GetChatHistoryQuery(Guid.NewGuid(), outsider);
        var swapRequest = TestData.SwapRequest(
            requesterId: TestData.UserId,
            receiverId: TestData.OtherUserId,
            status: SwapRequestStatus.Accepted);
        _fixture.Conversations
            .GetSwapRequestByConversationAsync(query.ConversationId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<SwapRequest?>(swapRequest));

        var handler = new GetChatHistoryQueryHandler(_fixture.UnitOfWork);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal("Chat.NotSwapParticipant", result.TopError.Code);
        Assert.Equal(ErrorKind.Forbidden, result.TopError.Type);
        await _fixture.Messages.DidNotReceive().GetPagedByConversationAsync(
            Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(SwapRequestStatus.Pending)]
    [InlineData(SwapRequestStatus.Rejected)]
    [InlineData(SwapRequestStatus.Cancelled)]
    public async Task Handle_ShouldReturnSwapNotActive_WhenSwapStatusIsNotAcceptedOrCompleted(SwapRequestStatus status)
    {
        var query = new GetChatHistoryQuery(Guid.NewGuid(), TestData.UserId);
        var swapRequest = TestData.SwapRequest(
            requesterId: TestData.UserId,
            receiverId: TestData.OtherUserId,
            status: status);
        _fixture.Conversations
            .GetSwapRequestByConversationAsync(query.ConversationId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<SwapRequest?>(swapRequest));

        var handler = new GetChatHistoryQueryHandler(_fixture.UnitOfWork);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal("Chat.SwapNotActive", result.TopError.Code);
        Assert.Equal(ErrorKind.Forbidden, result.TopError.Type);
        await _fixture.Messages.DidNotReceive().GetPagedByConversationAsync(
            Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnPagedMessagesInRepositoryOrder_WhenUserIsRequester()
    {
        var query = new GetChatHistoryQuery(Guid.NewGuid(), TestData.UserId, 2, 5);
        var swapRequest = TestData.SwapRequest(
            requesterId: TestData.UserId,
            receiverId: TestData.OtherUserId,
            status: SwapRequestStatus.Accepted);
        _fixture.Conversations
            .GetSwapRequestByConversationAsync(query.ConversationId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<SwapRequest?>(swapRequest));

        var first = TestData.Message(conversationId: query.ConversationId, senderId: TestData.OtherUserId,
            content: "Hi there!", isRead: true);
        var second = TestData.Message(conversationId: query.ConversationId, senderId: TestData.UserId,
            content: "Hello back!", isRead: false);
        var messages = new List<Message> { first, second };
        _fixture.Messages
            .GetPagedByConversationAsync(query.ConversationId, query.PageNumber, query.PageSize, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<(IReadOnlyList<Message>, int)>((messages, 12)));

        var handler = new GetChatHistoryQueryHandler(_fixture.UnitOfWork);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.False(result.IsError);
        Assert.Equal(12, result.Value.TotalCount);
        Assert.Equal(2, result.Value.PageNumber);
        Assert.Equal(5, result.Value.PageSize);
        Assert.Equal(3, result.Value.TotalPages);
        Assert.Equal(2, result.Value.Items.Count);

        Assert.Equal(first.Id, result.Value.Items[0].Id);
        Assert.Equal(query.ConversationId, result.Value.Items[0].ConversationId);
        Assert.Equal(TestData.OtherUserId, result.Value.Items[0].SenderId);
        Assert.Equal("Hi there!", result.Value.Items[0].Content);
        Assert.True(result.Value.Items[0].IsRead);
        Assert.Equal(first.SentAtUtc, result.Value.Items[0].SentAtUtc);

        Assert.Equal(second.Id, result.Value.Items[1].Id);
        Assert.Equal(TestData.UserId, result.Value.Items[1].SenderId);
        Assert.Equal("Hello back!", result.Value.Items[1].Content);
        Assert.False(result.Value.Items[1].IsRead);

        await _fixture.Messages.Received(1).GetPagedByConversationAsync(
            query.ConversationId, 2, 5, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnPagedMessages_WhenUserIsReceiver()
    {
        var query = new GetChatHistoryQuery(Guid.NewGuid(), TestData.OtherUserId, 1, 10);
        var swapRequest = TestData.SwapRequest(
            requesterId: TestData.UserId,
            receiverId: TestData.OtherUserId,
            status: SwapRequestStatus.Completed);
        _fixture.Conversations
            .GetSwapRequestByConversationAsync(query.ConversationId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<SwapRequest?>(swapRequest));
        _fixture.Messages
            .GetPagedByConversationAsync(query.ConversationId, query.PageNumber, query.PageSize, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<(IReadOnlyList<Message>, int)>((new List<Message>(), 0)));

        var handler = new GetChatHistoryQueryHandler(_fixture.UnitOfWork);
        var result = await handler.Handle(query, CancellationToken.None);

        Assert.False(result.IsError);
        Assert.Empty(result.Value.Items);
        Assert.Equal(0, result.Value.TotalCount);
        Assert.Equal(1, result.Value.PageNumber);
        Assert.Equal(10, result.Value.PageSize);
    }
}
