using NSubstitute;
using SkillSwapAPI.Application.Common.Interfaces.Services;
using SkillSwapAPI.Application.Features.LiveSessions.Commands.JoinLiveSession;
using SkillSwapAPI.Application.UnitTests.Common;
using SkillSwapAPI.Domain.Modules.LiveSessions.Entities;
using SkillSwapAPI.Domain.Modules.LiveSessions.Enums;
using SkillSwapAPI.Domain.Modules.SwapRequests.Enums;
using Xunit;

namespace SkillSwapAPI.Application.UnitTests.Features.LiveSessions;

public sealed class JoinLiveSessionCommandHandlerTests
{
    [Fact]
    public async Task JoinSession_ForAcceptedSwapParticipant_CreatesWaitingRoomOnce()
    {
        var fixture = new UnitOfWorkFixture();
        var tokens = Substitute.For<ILiveSessionTokenProvider>();
        tokens.GenerateRoomToken().Returns("opaque-room-token");
        var swap = TestData.SwapRequest(status: SwapRequestStatus.Accepted);
        fixture.SwapRequests.FindAsync(Arg.Any<System.Linq.Expressions.Expression<Func<SkillSwapAPI.Domain.Modules.SwapRequests.Entities.SwapRequest, bool>>>(), Arg.Any<CancellationToken>()).Returns(swap);
        fixture.LiveSessionRooms.GetBySwapRequestIdAsync(swap.Id, Arg.Any<CancellationToken>()).Returns((LiveSessionRoom?)null);
        LiveSessionRoom? created = null;
        fixture.LiveSessionRooms.AddAsync(Arg.Do<LiveSessionRoom>(room => created = room), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        var result = await new JoinLiveSessionCommandHandler(fixture.UnitOfWork, tokens)
            .Handle(new JoinLiveSessionCommand(swap.Id, swap.ReceiverId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("opaque-room-token", result.Value.RoomToken);
        Assert.Equal(LiveSessionStatus.Waiting, created?.Status);
        Assert.Equal(swap.Id, created?.SwapRequestId);
        await fixture.UnitOfWork.Received(1).CompleteAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task JoinSession_WhenParticipantRejoins_ReturnsExistingRoomWithoutCreatingAnother()
    {
        var fixture = new UnitOfWorkFixture();
        var tokens = Substitute.For<ILiveSessionTokenProvider>();
        var swap = TestData.SwapRequest(status: SwapRequestStatus.Accepted);
        var existing = new LiveSessionRoom { Id = Guid.NewGuid(), SwapRequestId = swap.Id, RoomToken = "existing-token", Status = LiveSessionStatus.InProgress };
        fixture.SwapRequests.FindAsync(Arg.Any<System.Linq.Expressions.Expression<Func<SkillSwapAPI.Domain.Modules.SwapRequests.Entities.SwapRequest, bool>>>(), Arg.Any<CancellationToken>()).Returns(swap);
        fixture.LiveSessionRooms.GetBySwapRequestIdAsync(swap.Id, Arg.Any<CancellationToken>()).Returns(existing);

        var result = await new JoinLiveSessionCommandHandler(fixture.UnitOfWork, tokens)
            .Handle(new JoinLiveSessionCommand(swap.Id, swap.RequesterId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(existing.Id, result.Value.Id);
        Assert.Equal("existing-token", result.Value.RoomToken);
        tokens.DidNotReceive().GenerateRoomToken();
        await fixture.LiveSessionRooms.DidNotReceive().AddAsync(Arg.Any<LiveSessionRoom>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task JoinSession_WhenCallerIsNotSwapParticipant_ReturnsErrorWithoutCreatingRoom()
    {
        var fixture = new UnitOfWorkFixture();
        var tokens = Substitute.For<ILiveSessionTokenProvider>();
        var swap = TestData.SwapRequest(status: SwapRequestStatus.Accepted);
        fixture.SwapRequests.FindAsync(Arg.Any<System.Linq.Expressions.Expression<Func<SkillSwapAPI.Domain.Modules.SwapRequests.Entities.SwapRequest, bool>>>(), Arg.Any<CancellationToken>()).Returns(swap);

        var result = await new JoinLiveSessionCommandHandler(fixture.UnitOfWork, tokens)
            .Handle(new JoinLiveSessionCommand(swap.Id, Guid.NewGuid()), CancellationToken.None);

        Assert.False(result.IsSuccess);
        await fixture.LiveSessionRooms.DidNotReceive().AddAsync(Arg.Any<LiveSessionRoom>(), Arg.Any<CancellationToken>());
        await fixture.UnitOfWork.DidNotReceive().CompleteAsync(Arg.Any<CancellationToken>());
    }
}
