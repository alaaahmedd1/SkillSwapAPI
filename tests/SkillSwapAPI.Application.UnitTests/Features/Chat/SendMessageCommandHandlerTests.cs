using NSubstitute;
using SkillSwapAPI.Application.Features.Chat.Commands.SendMessage;
using SkillSwapAPI.Application.UnitTests.Common;
using SkillSwapAPI.Domain.Common.Results;
using SkillSwapAPI.Domain.Modules.Chat.Entities;
using SkillSwapAPI.Domain.Modules.SwapRequests.Entities;
using SkillSwapAPI.Domain.Modules.SwapRequests.Enums;
using Xunit;

namespace SkillSwapAPI.Application.UnitTests.Features.Chat;

public class SendMessageCommandHandlerTests
{
    private readonly UnitOfWorkFixture _fixture = new();

    [Fact]
    public async Task Handle_ShouldReturnConversationNotFound_WhenSwapRequestIsMissing()
    {
        var command = new SendMessageCommand(Guid.NewGuid(), TestData.UserId, "Hello!");
        _fixture.Conversations
            .GetSwapRequestByConversationAsync(command.ConversationId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<SwapRequest?>(null));

        var handler = new SendMessageCommandHandler(_fixture.UnitOfWork);
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal("Chat.ConversationNotFound", result.TopError.Code);
        Assert.Equal(ErrorKind.NotFound, result.TopError.Type);
        await _fixture.Messages.DidNotReceive().AddAsync(Arg.Any<Message>(), Arg.Any<CancellationToken>());
        await _fixture.UnitOfWork.DidNotReceive().CompleteAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnNotSwapParticipant_WhenSenderIsNeitherRequesterNorReceiver()
    {
        var outsider = Guid.NewGuid();
        var command = new SendMessageCommand(Guid.NewGuid(), outsider, "Hello!");
        var swapRequest = TestData.SwapRequest(
            requesterId: TestData.UserId,
            receiverId: TestData.OtherUserId,
            status: SwapRequestStatus.Accepted);
        _fixture.Conversations
            .GetSwapRequestByConversationAsync(command.ConversationId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<SwapRequest?>(swapRequest));

        var handler = new SendMessageCommandHandler(_fixture.UnitOfWork);
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal("Chat.NotSwapParticipant", result.TopError.Code);
        Assert.Equal(ErrorKind.Forbidden, result.TopError.Type);
        await _fixture.Messages.DidNotReceive().AddAsync(Arg.Any<Message>(), Arg.Any<CancellationToken>());
        await _fixture.UnitOfWork.DidNotReceive().CompleteAsync(Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(SwapRequestStatus.Pending)]
    [InlineData(SwapRequestStatus.Rejected)]
    [InlineData(SwapRequestStatus.Cancelled)]
    public async Task Handle_ShouldReturnSwapNotActive_WhenSwapStatusIsNotAcceptedOrCompleted(SwapRequestStatus status)
    {
        var command = new SendMessageCommand(Guid.NewGuid(), TestData.UserId, "Hello!");
        var swapRequest = TestData.SwapRequest(
            requesterId: TestData.UserId,
            receiverId: TestData.OtherUserId,
            status: status);
        _fixture.Conversations
            .GetSwapRequestByConversationAsync(command.ConversationId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<SwapRequest?>(swapRequest));

        var handler = new SendMessageCommandHandler(_fixture.UnitOfWork);
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal("Chat.SwapNotActive", result.TopError.Code);
        Assert.Equal(ErrorKind.Forbidden, result.TopError.Type);
        await _fixture.Messages.DidNotReceive().AddAsync(Arg.Any<Message>(), Arg.Any<CancellationToken>());
        await _fixture.UnitOfWork.DidNotReceive().CompleteAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldPersistUnreadMessageAndReturnDto_WhenSwapIsAcceptedAndSenderIsRequester()
    {
        var command = new SendMessageCommand(Guid.NewGuid(), TestData.UserId, "Hello there!");
        var swapRequest = TestData.SwapRequest(
            requesterId: TestData.UserId,
            receiverId: TestData.OtherUserId,
            status: SwapRequestStatus.Accepted);
        _fixture.Conversations
            .GetSwapRequestByConversationAsync(command.ConversationId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<SwapRequest?>(swapRequest));
        _fixture.Messages
            .AddAsync(Arg.Any<Message>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var handler = new SendMessageCommandHandler(_fixture.UnitOfWork);
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsError);
        Assert.NotEqual(Guid.Empty, result.Value.Id);
        Assert.Equal(command.ConversationId, result.Value.ConversationId);
        Assert.Equal(command.SenderId, result.Value.SenderId);
        Assert.Equal(command.Content, result.Value.Content);
        Assert.False(result.Value.IsRead);
        await _fixture.Messages.Received(1).AddAsync(
            Arg.Is<Message>(message =>
                message.Id != Guid.Empty &&
                message.ConversationId == command.ConversationId &&
                message.SenderId == command.SenderId &&
                message.Content == command.Content &&
                !message.IsRead),
            Arg.Any<CancellationToken>());
        await _fixture.UnitOfWork.Received(1).CompleteAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldPersistMessage_WhenSwapIsCompletedAndSenderIsReceiver()
    {
        var command = new SendMessageCommand(Guid.NewGuid(), TestData.OtherUserId, "Thanks!");
        var swapRequest = TestData.SwapRequest(
            requesterId: TestData.UserId,
            receiverId: TestData.OtherUserId,
            status: SwapRequestStatus.Completed);
        _fixture.Conversations
            .GetSwapRequestByConversationAsync(command.ConversationId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<SwapRequest?>(swapRequest));
        _fixture.Messages
            .AddAsync(Arg.Any<Message>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var handler = new SendMessageCommandHandler(_fixture.UnitOfWork);
        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsError);
        Assert.Equal(command.Content, result.Value.Content);
        Assert.Equal(command.SenderId, result.Value.SenderId);
        await _fixture.Messages.Received(1).AddAsync(
            Arg.Is<Message>(message =>
                message.ConversationId == command.ConversationId &&
                message.SenderId == command.SenderId),
            Arg.Any<CancellationToken>());
        await _fixture.UnitOfWork.Received(1).CompleteAsync(Arg.Any<CancellationToken>());
    }
}
