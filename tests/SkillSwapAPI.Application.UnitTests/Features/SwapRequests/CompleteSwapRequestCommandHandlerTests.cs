using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using SkillSwapAPI.Application.Common.Errors;
using SkillSwapAPI.Application.Features.SwapRequests.Commands.CompleteSwapRequest;
using SkillSwapAPI.Application.UnitTests.Common;
using SkillSwapAPI.Domain.Common.Results;
using SkillSwapAPI.Domain.Modules.SwapRequests.Entities;
using SkillSwapAPI.Domain.Modules.SwapRequests.Enums;
using Xunit;

namespace SkillSwapAPI.Application.UnitTests.Features.SwapRequests;

public class CompleteSwapRequestCommandHandlerTests
{
    private readonly UnitOfWorkFixture _fixture = new();

    private readonly Guid _swapRequestId = Guid.NewGuid();
    private readonly Guid _requesterId = TestData.UserId;
    private readonly Guid _receiverId = TestData.OtherUserId;

    private CompleteSwapRequestCommandHandler CreateHandler() => new(_fixture.UnitOfWork);

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
        var command = new CompleteSwapRequestCommand(_swapRequestId, _requesterId);

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal("SwapRequests.NotFound", result.TopError.Code);
        Assert.Equal(ErrorKind.NotFound, result.TopError.Type);
        _fixture.SwapRequests.DidNotReceive().Update(Arg.Any<SwapRequest>());
        await _fixture.UnitOfWork.DidNotReceive().CompleteAsync(Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(SwapRequestStatus.Pending)]
    [InlineData(SwapRequestStatus.Rejected)]
    [InlineData(SwapRequestStatus.Cancelled)]
    [InlineData(SwapRequestStatus.Completed)]
    public async Task Handle_ShouldReturnInvalidStatusTransition_WhenSwapRequestIsNotAccepted(SwapRequestStatus status)
    {
        var swap = TestData.SwapRequest(_swapRequestId, _requesterId, _receiverId, status: status);
        SetupSwapRequest(swap);
        var command = new CompleteSwapRequestCommand(_swapRequestId, _requesterId);

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal("SwapRequests.InvalidStatusTransition", result.TopError.Code);
        Assert.Equal(ErrorKind.Conflict, result.TopError.Type);
        Assert.Equal(status, swap.Status);
        Assert.False(swap.IsRequesterConfirmed);
        Assert.False(swap.IsReceiverConfirmed);
        _fixture.SwapRequests.DidNotReceive().Update(Arg.Any<SwapRequest>());
        await _fixture.UnitOfWork.DidNotReceive().CompleteAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnNotParticipant_WhenCallerIsNotParticipant()
    {
        var swap = TestData.SwapRequest(_swapRequestId, _requesterId, _receiverId, status: SwapRequestStatus.Accepted);
        SetupSwapRequest(swap);
        var command = new CompleteSwapRequestCommand(_swapRequestId, Guid.NewGuid());

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal("SwapRequests.NotParticipant", result.TopError.Code);
        Assert.Equal(ErrorKind.Forbidden, result.TopError.Type);
        Assert.Equal(SwapRequestStatus.Accepted, swap.Status);
        Assert.False(swap.IsRequesterConfirmed);
        Assert.False(swap.IsReceiverConfirmed);
        _fixture.SwapRequests.DidNotReceive().Update(Arg.Any<SwapRequest>());
        await _fixture.UnitOfWork.DidNotReceive().CompleteAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldConfirmRequesterOnly_WhenRequesterCompletesFirst()
    {
        var swap = TestData.SwapRequest(_swapRequestId, _requesterId, _receiverId, status: SwapRequestStatus.Accepted);
        SetupSwapRequest(swap);
        var command = new CompleteSwapRequestCommand(_swapRequestId, _requesterId);

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.False(result.IsError);
        Assert.Equal(Result.Updated, result.Value);
        Assert.True(swap.IsRequesterConfirmed);
        Assert.False(swap.IsReceiverConfirmed);
        Assert.Equal(SwapRequestStatus.Accepted, swap.Status);
        Assert.Null(swap.UpdatedAtUtc);
        _fixture.SwapRequests.Received(1).Update(swap);
        await _fixture.UnitOfWork.Received(1).CompleteAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldConfirmReceiverOnly_WhenReceiverCompletesFirst()
    {
        var swap = TestData.SwapRequest(_swapRequestId, _requesterId, _receiverId, status: SwapRequestStatus.Accepted);
        SetupSwapRequest(swap);
        var command = new CompleteSwapRequestCommand(_swapRequestId, _receiverId);

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.False(result.IsError);
        Assert.False(swap.IsRequesterConfirmed);
        Assert.True(swap.IsReceiverConfirmed);
        Assert.Equal(SwapRequestStatus.Accepted, swap.Status);
        Assert.Null(swap.UpdatedAtUtc);
        _fixture.SwapRequests.Received(1).Update(swap);
        await _fixture.UnitOfWork.Received(1).CompleteAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldCompleteSwapRequest_WhenRequesterConfirmsAfterReceiver()
    {
        var swap = TestData.SwapRequest(
            _swapRequestId, _requesterId, _receiverId,
            status: SwapRequestStatus.Accepted, receiverConfirmed: true);
        SetupSwapRequest(swap);
        var command = new CompleteSwapRequestCommand(_swapRequestId, _requesterId);

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.False(result.IsError);
        Assert.True(swap.IsRequesterConfirmed);
        Assert.True(swap.IsReceiverConfirmed);
        Assert.Equal(SwapRequestStatus.Completed, swap.Status);
        Assert.NotNull(swap.UpdatedAtUtc);
        _fixture.SwapRequests.Received(1).Update(swap);
        await _fixture.UnitOfWork.Received(1).CompleteAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldCompleteSwapRequest_WhenReceiverConfirmsAfterRequester()
    {
        var swap = TestData.SwapRequest(
            _swapRequestId, _requesterId, _receiverId,
            status: SwapRequestStatus.Accepted, requesterConfirmed: true);
        SetupSwapRequest(swap);
        var command = new CompleteSwapRequestCommand(_swapRequestId, _receiverId);

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.False(result.IsError);
        Assert.True(swap.IsRequesterConfirmed);
        Assert.True(swap.IsReceiverConfirmed);
        Assert.Equal(SwapRequestStatus.Completed, swap.Status);
        Assert.NotNull(swap.UpdatedAtUtc);
        _fixture.SwapRequests.Received(1).Update(swap);
        await _fixture.UnitOfWork.Received(1).CompleteAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnConcurrencyConflict_WhenCompleteAsyncThrowsConcurrencyException()
    {
        var swap = TestData.SwapRequest(_swapRequestId, _requesterId, _receiverId, status: SwapRequestStatus.Accepted);
        SetupSwapRequest(swap);
        _fixture.UnitOfWork
            .CompleteAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException<int>(new DbUpdateConcurrencyException(
                "The swap request was modified by another request.",
                new InvalidOperationException("RowVersion mismatch."))));
        var command = new CompleteSwapRequestCommand(_swapRequestId, _requesterId);

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        Assert.True(result.IsError);
        Assert.Equal("SwapRequests.ConcurrencyConflict", result.TopError.Code);
        Assert.Equal(ErrorKind.Conflict, result.TopError.Type);
        Assert.True(swap.IsRequesterConfirmed);
        _fixture.SwapRequests.Received(1).Update(swap);
        await _fixture.UnitOfWork.Received(1).CompleteAsync(Arg.Any<CancellationToken>());
    }
}
