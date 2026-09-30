using NSubstitute;
using SkillSwapAPI.Application.Features.LiveSessions.Commands.StartLiveSession;
using SkillSwapAPI.Application.UnitTests.Common;
using SkillSwapAPI.Domain.Modules.LiveSessions.Entities;
using SkillSwapAPI.Domain.Modules.LiveSessions.Enums;
using SkillSwapAPI.Domain.Modules.SwapRequests.Enums;
using Xunit;

namespace SkillSwapAPI.Application.UnitTests.Features.LiveSessions;

public sealed class StartLiveSessionCommandHandlerTests
{
    [Fact]
    public async Task StartSession_ForAcceptedParticipant_TransitionsWaitingRoomToInProgress()
    {
        var fixture = new UnitOfWorkFixture();
        var swap = TestData.SwapRequest(status: SwapRequestStatus.Accepted);
        var room = new LiveSessionRoom { Id = Guid.NewGuid(), SwapRequestId = swap.Id, RoomToken = "room", Status = LiveSessionStatus.Waiting };
        fixture.SwapRequests.FindAsync(Arg.Any<System.Linq.Expressions.Expression<Func<SkillSwapAPI.Domain.Modules.SwapRequests.Entities.SwapRequest, bool>>>(), Arg.Any<CancellationToken>()).Returns(swap);
        fixture.LiveSessionRooms.GetBySwapRequestIdAsync(swap.Id, Arg.Any<CancellationToken>()).Returns(room);

        var result = await new StartLiveSessionCommandHandler(fixture.UnitOfWork)
            .Handle(new StartLiveSessionCommand(swap.Id, swap.RequesterId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(LiveSessionStatus.InProgress, room.Status);
        Assert.NotNull(room.ActualStartTime);
        fixture.LiveSessionRooms.Received(1).Update(room);
        await fixture.UnitOfWork.Received(1).CompleteAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task StartSession_WhenCallerIsNotParticipant_DoesNotStartRoom()
    {
        var fixture = new UnitOfWorkFixture();
        var swap = TestData.SwapRequest(status: SwapRequestStatus.Accepted);
        var room = new LiveSessionRoom { Id = Guid.NewGuid(), SwapRequestId = swap.Id, RoomToken = "room", Status = LiveSessionStatus.Waiting };
        fixture.SwapRequests.FindAsync(Arg.Any<System.Linq.Expressions.Expression<Func<SkillSwapAPI.Domain.Modules.SwapRequests.Entities.SwapRequest, bool>>>(), Arg.Any<CancellationToken>()).Returns(swap);
        fixture.LiveSessionRooms.GetBySwapRequestIdAsync(swap.Id, Arg.Any<CancellationToken>()).Returns(room);

        var result = await new StartLiveSessionCommandHandler(fixture.UnitOfWork)
            .Handle(new StartLiveSessionCommand(swap.Id, Guid.NewGuid()), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(LiveSessionStatus.Waiting, room.Status);
        fixture.LiveSessionRooms.DidNotReceive().Update(Arg.Any<LiveSessionRoom>());
        await fixture.UnitOfWork.DidNotReceive().CompleteAsync(Arg.Any<CancellationToken>());
    }
}
