using System.Linq.Expressions;
using NSubstitute;
using SkillSwapAPI.Application.Common.Errors;
using SkillSwapAPI.Application.Features.SwapRequests.Commands.CancelSwapRequest;
using SkillSwapAPI.Application.UnitTests.Common;
using SkillSwapAPI.Domain.Common.Results;
using SkillSwapAPI.Domain.Modules.SwapRequests.Entities;
using SkillSwapAPI.Domain.Modules.SwapRequests.Enums;
using Xunit;

namespace SkillSwapAPI.Application.UnitTests.Features.SwapRequests;

public class CancelSwapRequestCommandHandlerTests
{
    private readonly UnitOfWorkFixture _fixture = new();

    private readonly Guid _swapRequestId = Guid.NewGuid();
    private readonly Guid _requesterId = TestData.UserId;
    private readonly Guid _receiverId = TestData.OtherUserId;

    private CancelSwapRequestCommandHandler CreateHandler() => new(_fixture.UnitOfWork);

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
        var command = new CancelSwapRequestCommand(_swapRequestId, _requesterId);

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal("SwapRequests.NotFound", result.TopError.Code);
        Assert.Equal(ErrorKind.NotFound, result.TopError.Type);
        _fixture.SwapRequests.DidNotReceive().Update(Arg.Any<SwapRequest>());
        await _fixture.UnitOfWork.DidNotReceive().CompleteAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnOnlyRequesterCanCancel_WhenReceiverCancelsPendingRequest()
    {
        var swap = TestData.SwapRequest(_swapRequestId, _requesterId, _receiverId);
        SetupSwapRequest(swap);
        var command = new CancelSwapRequestCommand(_swapRequestId, _receiverId);

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal("SwapRequests.OnlyRequesterCanCancel", result.TopError.Code);
        Assert.Equal(ErrorKind.Forbidden, result.TopError.Type);
        Assert.Equal(SwapRequestStatus.Pending, swap.Status);
        _fixture.SwapRequests.DidNotReceive().Update(Arg.Any<SwapRequest>());
        await _fixture.UnitOfWork.DidNotReceive().CompleteAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnOnlyRequesterCanCancel_WhenThirdPartyCancelsPendingRequest()
    {
        var swap = TestData.SwapRequest(_swapRequestId, _requesterId, _receiverId);
        SetupSwapRequest(swap);
        var command = new CancelSwapRequestCommand(_swapRequestId, Guid.NewGuid());

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal("SwapRequests.OnlyRequesterCanCancel", result.TopError.Code);
        Assert.Equal(ErrorKind.Forbidden, result.TopError.Type);
        Assert.Equal(SwapRequestStatus.Pending, swap.Status);
        _fixture.SwapRequests.DidNotReceive().Update(Arg.Any<SwapRequest>());
        await _fixture.UnitOfWork.DidNotReceive().CompleteAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnNotParticipant_WhenThirdPartyCancelsAcceptedRequest()
    {
        var swap = TestData.SwapRequest(_swapRequestId, _requesterId, _receiverId, status: SwapRequestStatus.Accepted);
        SetupSwapRequest(swap);
        var command = new CancelSwapRequestCommand(_swapRequestId, Guid.NewGuid());

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal("SwapRequests.NotParticipant", result.TopError.Code);
        Assert.Equal(ErrorKind.Forbidden, result.TopError.Type);
        Assert.Equal(SwapRequestStatus.Accepted, swap.Status);
        _fixture.SwapRequests.DidNotReceive().Update(Arg.Any<SwapRequest>());
        await _fixture.UnitOfWork.DidNotReceive().CompleteAsync(Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(SwapRequestStatus.Rejected)]
    [InlineData(SwapRequestStatus.Cancelled)]
    [InlineData(SwapRequestStatus.Completed)]
    public async Task Handle_ShouldReturnInvalidStatusTransition_WhenSwapRequestIsInTerminalState(SwapRequestStatus status)
    {
        var swap = TestData.SwapRequest(_swapRequestId, _requesterId, _receiverId, status: status);
        SetupSwapRequest(swap);
        var command = new CancelSwapRequestCommand(_swapRequestId, _requesterId);

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal("SwapRequests.InvalidStatusTransition", result.TopError.Code);
        Assert.Equal(ErrorKind.Conflict, result.TopError.Type);
        Assert.Equal(status, swap.Status);
        _fixture.SwapRequests.DidNotReceive().Update(Arg.Any<SwapRequest>());
        await _fixture.UnitOfWork.DidNotReceive().CompleteAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldCancelPendingSwapRequest_WhenRequesterCancels()
    {
        var swap = TestData.SwapRequest(_swapRequestId, _requesterId, _receiverId);
        SetupSwapRequest(swap);
        var command = new CancelSwapRequestCommand(_swapRequestId, _requesterId);

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.False(result.IsError);
        Assert.Equal(Result.Updated, result.Value);
        Assert.Equal(SwapRequestStatus.Cancelled, swap.Status);
        Assert.NotNull(swap.UpdatedAtUtc);
        _fixture.SwapRequests.Received(1).Update(swap);
        await _fixture.UnitOfWork.Received(1).CompleteAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldCancelAcceptedSwapRequest_WhenRequesterCancels()
    {
        var swap = TestData.SwapRequest(_swapRequestId, _requesterId, _receiverId, status: SwapRequestStatus.Accepted);
        SetupSwapRequest(swap);
        var command = new CancelSwapRequestCommand(_swapRequestId, _requesterId);

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.False(result.IsError);
        Assert.Equal(SwapRequestStatus.Cancelled, swap.Status);
        Assert.NotNull(swap.UpdatedAtUtc);
        _fixture.SwapRequests.Received(1).Update(swap);
        await _fixture.UnitOfWork.Received(1).CompleteAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldCancelAcceptedSwapRequest_WhenReceiverCancels()
    {
        var swap = TestData.SwapRequest(_swapRequestId, _requesterId, _receiverId, status: SwapRequestStatus.Accepted);
        SetupSwapRequest(swap);
        var command = new CancelSwapRequestCommand(_swapRequestId, _receiverId);

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.False(result.IsError);
        Assert.Equal(SwapRequestStatus.Cancelled, swap.Status);
        Assert.NotNull(swap.UpdatedAtUtc);
        _fixture.SwapRequests.Received(1).Update(swap);
        await _fixture.UnitOfWork.Received(1).CompleteAsync(Arg.Any<CancellationToken>());
    }
}
