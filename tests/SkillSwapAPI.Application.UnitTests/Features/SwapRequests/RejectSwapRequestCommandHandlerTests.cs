using System.Linq.Expressions;
using NSubstitute;
using SkillSwapAPI.Application.Common.Errors;
using SkillSwapAPI.Application.Features.SwapRequests.Commands.RejectSwapRequest;
using SkillSwapAPI.Application.UnitTests.Common;
using SkillSwapAPI.Domain.Common.Results;
using SkillSwapAPI.Domain.Modules.Chat.Entities;
using SkillSwapAPI.Domain.Modules.SwapRequests.Entities;
using SkillSwapAPI.Domain.Modules.SwapRequests.Enums;
using Xunit;

namespace SkillSwapAPI.Application.UnitTests.Features.SwapRequests;

public class RejectSwapRequestCommandHandlerTests
{
    private readonly UnitOfWorkFixture _fixture = new();

    private readonly Guid _swapRequestId = Guid.NewGuid();
    private readonly Guid _requesterId = TestData.UserId;
    private readonly Guid _receiverId = TestData.OtherUserId;

    private RejectSwapRequestCommandHandler CreateHandler() => new(_fixture.UnitOfWork);

    private void SetupSwapRequest(SwapRequest? swapRequest)
    {
        _fixture.SwapRequests
            .FindAsync(Arg.Any<Expression<Func<SwapRequest, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<SwapRequest?>(swapRequest));
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenSwapRequestDoesNotExist()
    {
        SetupSwapRequest(null);
        var command = new RejectSwapRequestCommand(_swapRequestId, _receiverId);

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal("SwapRequests.NotFound", result.TopError.Code);
        Assert.Equal(ErrorKind.NotFound, result.TopError.Type);
        _fixture.SwapRequests.DidNotReceive().Update(Arg.Any<SwapRequest>());
        await _fixture.UnitOfWork.DidNotReceive().CompleteAsync(Arg.Any<CancellationToken>());
        await _fixture.Conversations.DidNotReceive().AddAsync(Arg.Any<Conversation>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(SwapRequestStatus.Accepted)]
    [InlineData(SwapRequestStatus.Rejected)]
    [InlineData(SwapRequestStatus.Cancelled)]
    [InlineData(SwapRequestStatus.Completed)]
    public async Task Handle_ShouldReturnInvalidStatusTransition_WhenSwapRequestIsNotPending(SwapRequestStatus status)
    {
        var swap = TestData.SwapRequest(_swapRequestId, _requesterId, _receiverId, status: status);
        SetupSwapRequest(swap);
        var command = new RejectSwapRequestCommand(_swapRequestId, _receiverId);

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal("SwapRequests.InvalidStatusTransition", result.TopError.Code);
        Assert.Equal(ErrorKind.Conflict, result.TopError.Type);
        Assert.Equal(status, swap.Status);
        _fixture.SwapRequests.DidNotReceive().Update(Arg.Any<SwapRequest>());
        await _fixture.UnitOfWork.DidNotReceive().CompleteAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnOnlyReceiverCanRespond_WhenCallerIsRequester()
    {
        var swap = TestData.SwapRequest(_swapRequestId, _requesterId, _receiverId);
        SetupSwapRequest(swap);
        var command = new RejectSwapRequestCommand(_swapRequestId, _requesterId);

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal("SwapRequests.OnlyReceiverCanRespond", result.TopError.Code);
        Assert.Equal(ErrorKind.Forbidden, result.TopError.Type);
        Assert.Equal(SwapRequestStatus.Pending, swap.Status);
        _fixture.SwapRequests.DidNotReceive().Update(Arg.Any<SwapRequest>());
        await _fixture.UnitOfWork.DidNotReceive().CompleteAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnOnlyReceiverCanRespond_WhenCallerIsNotParticipant()
    {
        var swap = TestData.SwapRequest(_swapRequestId, _requesterId, _receiverId);
        SetupSwapRequest(swap);
        var command = new RejectSwapRequestCommand(_swapRequestId, Guid.NewGuid());

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal("SwapRequests.OnlyReceiverCanRespond", result.TopError.Code);
        Assert.Equal(ErrorKind.Forbidden, result.TopError.Type);
        Assert.Equal(SwapRequestStatus.Pending, swap.Status);
        _fixture.SwapRequests.DidNotReceive().Update(Arg.Any<SwapRequest>());
        await _fixture.UnitOfWork.DidNotReceive().CompleteAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldRejectPendingSwapRequest_WhenReceiverRejects()
    {
        var swap = TestData.SwapRequest(_swapRequestId, _requesterId, _receiverId);
        SetupSwapRequest(swap);
        var command = new RejectSwapRequestCommand(_swapRequestId, _receiverId);

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.False(result.IsError);
        Assert.Equal(Result.Updated, result.Value);
        Assert.Equal(SwapRequestStatus.Rejected, swap.Status);
        Assert.NotNull(swap.UpdatedAtUtc);
        _fixture.SwapRequests.Received(1).Update(swap);
        await _fixture.UnitOfWork.Received(1).CompleteAsync(Arg.Any<CancellationToken>());
        await _fixture.Conversations.DidNotReceive().AddAsync(Arg.Any<Conversation>(), Arg.Any<CancellationToken>());
    }
}
