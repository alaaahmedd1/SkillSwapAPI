using NSubstitute;
using SkillSwapAPI.Application.Common.Interfaces.Services;
using SkillSwapAPI.Application.Features.LiveSessions.Commands.EndLiveSession;
using SkillSwapAPI.Application.UnitTests.Common;
using SkillSwapAPI.Domain.Common.Results;
using SkillSwapAPI.Domain.Modules.LiveSessions.Enums;
using SkillSwapAPI.Domain.Modules.SwapRequests.Enums;
using Xunit;

namespace SkillSwapAPI.Application.UnitTests.Features.LiveSessions;

public sealed class EndLiveSessionCommandHandlerTests
{
    [Fact]
    public async Task EndSession_WhenRoomHasElapsedTime_SettlesRoundedMinutesAndEndsRoom()
    {
        var fixture = new UnitOfWorkFixture();
        var ledger = Substitute.For<ITimeLedgerService>();
        var swap = TestData.SwapRequest(status: SwapRequestStatus.Accepted);
        var room = new SkillSwapAPI.Domain.Modules.LiveSessions.Entities.LiveSessionRoom
        {
            Id = Guid.NewGuid(), SwapRequestId = swap.Id, RoomToken = "room-token",
            ScheduledStartTime = DateTimeOffset.UtcNow.AddMinutes(-2),
            ActualStartTime = DateTimeOffset.UtcNow.AddMinutes(-2), Status = LiveSessionStatus.InProgress
        };
        fixture.LiveSessionRooms.GetByIdAsync(room.Id).Returns(room);
        fixture.SwapRequests.FindAsync(Arg.Any<System.Linq.Expressions.Expression<Func<SkillSwapAPI.Domain.Modules.SwapRequests.Entities.SwapRequest, bool>>>(), Arg.Any<CancellationToken>()).Returns(swap);

        var result = await new EndLiveSessionCommandHandler(fixture.UnitOfWork, ledger)
            .Handle(new EndLiveSessionCommand(room.Id, swap.RequesterId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(LiveSessionStatus.Ended, room.Status);
        Assert.NotNull(room.ActualEndTime);
        Assert.InRange(room.DurationSeconds, 115, 130);
        await ledger.Received(1).ValidateSettlementAsync(swap.RequesterId, swap.ReceiverId, 2, Arg.Any<CancellationToken>());
        await ledger.Received(1).SettleAsync(swap.RequesterId, swap.ReceiverId, 2, swap.Id, Arg.Any<CancellationToken>());
        await fixture.UnitOfWork.Received(1).CompleteAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EndSession_WhenAlreadyEnded_IsIdempotentAndDoesNotSettleAgain()
    {
        var fixture = new UnitOfWorkFixture();
        var ledger = Substitute.For<ITimeLedgerService>();
        var swap = TestData.SwapRequest(status: SwapRequestStatus.Accepted);
        var endedAt = DateTimeOffset.UtcNow.AddMinutes(-1);
        var room = new SkillSwapAPI.Domain.Modules.LiveSessions.Entities.LiveSessionRoom
        {
            Id = Guid.NewGuid(), SwapRequestId = swap.Id, RoomToken = "room-token",
            Status = LiveSessionStatus.Ended, ActualEndTime = endedAt, DurationSeconds = 60
        };
        fixture.LiveSessionRooms.GetByIdAsync(room.Id).Returns(room);
        fixture.SwapRequests.FindAsync(Arg.Any<System.Linq.Expressions.Expression<Func<SkillSwapAPI.Domain.Modules.SwapRequests.Entities.SwapRequest, bool>>>(), Arg.Any<CancellationToken>()).Returns(swap);

        var result = await new EndLiveSessionCommandHandler(fixture.UnitOfWork, ledger)
            .Handle(new EndLiveSessionCommand(room.Id, swap.RequesterId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(endedAt, result.Value.ActualEndTime);
        await ledger.DidNotReceive().SettleAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>());
        await fixture.UnitOfWork.DidNotReceive().CompleteAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EndSession_WhenCallerIsNotParticipant_ReturnsErrorWithoutSettling()
    {
        var fixture = new UnitOfWorkFixture();
        var ledger = Substitute.For<ITimeLedgerService>();
        var swap = TestData.SwapRequest(status: SwapRequestStatus.Accepted);
        var room = new SkillSwapAPI.Domain.Modules.LiveSessions.Entities.LiveSessionRoom
        {
            Id = Guid.NewGuid(), SwapRequestId = swap.Id, RoomToken = "room-token", Status = LiveSessionStatus.InProgress,
            ActualStartTime = DateTimeOffset.UtcNow.AddMinutes(-3)
        };
        fixture.LiveSessionRooms.GetByIdAsync(room.Id).Returns(room);
        fixture.SwapRequests.FindAsync(Arg.Any<System.Linq.Expressions.Expression<Func<SkillSwapAPI.Domain.Modules.SwapRequests.Entities.SwapRequest, bool>>>(), Arg.Any<CancellationToken>()).Returns(swap);

        var result = await new EndLiveSessionCommandHandler(fixture.UnitOfWork, ledger)
            .Handle(new EndLiveSessionCommand(room.Id, Guid.NewGuid()), CancellationToken.None);

        Assert.False(result.IsSuccess);
        await ledger.DidNotReceive().SettleAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<int>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>());
        await fixture.UnitOfWork.DidNotReceive().CompleteAsync(Arg.Any<CancellationToken>());
    }
}
