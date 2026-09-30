using NSubstitute;
using SkillSwapAPI.Application.Features.LiveSessions.Commands.SaveWhiteboardSnapshot;
using SkillSwapAPI.Application.UnitTests.Common;
using SkillSwapAPI.Domain.Modules.LiveSessions.Entities;
using SkillSwapAPI.Domain.Modules.LiveSessions.Enums;
using SkillSwapAPI.Domain.Modules.SwapRequests.Enums;
using Xunit;

namespace SkillSwapAPI.Application.UnitTests.Features.LiveSessions;

public sealed class WhiteboardSnapshotCommandHandlerTests
{
    [Fact]
    public async Task SaveSnapshot_ForSwapParticipant_UpsertsCanvasForLiveRoom()
    {
        var fixture = new UnitOfWorkFixture();
        var swap = TestData.SwapRequest(status: SwapRequestStatus.Accepted);
        var room = new LiveSessionRoom { Id = Guid.NewGuid(), SwapRequestId = swap.Id, RoomToken = "room", Status = LiveSessionStatus.InProgress };
        fixture.SwapRequests.FindAsync(Arg.Any<System.Linq.Expressions.Expression<Func<SkillSwapAPI.Domain.Modules.SwapRequests.Entities.SwapRequest, bool>>>(), Arg.Any<CancellationToken>()).Returns(swap);
        fixture.LiveSessionRooms.GetBySwapRequestIdAsync(swap.Id, Arg.Any<CancellationToken>()).Returns(room);
        WhiteboardSnapshot? created = null;
        fixture.WhiteboardSnapshots.AddAsync(Arg.Do<WhiteboardSnapshot>(snapshot => created = snapshot), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        var result = await new SaveWhiteboardSnapshotCommandHandler(fixture.UnitOfWork)
            .Handle(new SaveWhiteboardSnapshotCommand(swap.Id, swap.RequesterId, "{\"items\":[1]}"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(room.Id, created?.RoomId);
        Assert.Equal("{\"items\":[1]}", result.Value.CanvasDataJson);
        Assert.NotEqual(default, result.Value.UpdatedAtUtc);
        await fixture.UnitOfWork.Received(1).CompleteAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SaveSnapshot_WhenExistingSnapshotExists_UpdatesItInsteadOfAddingDuplicate()
    {
        var fixture = new UnitOfWorkFixture();
        var swap = TestData.SwapRequest(status: SwapRequestStatus.Accepted);
        var room = new LiveSessionRoom { Id = Guid.NewGuid(), SwapRequestId = swap.Id, RoomToken = "room", Status = LiveSessionStatus.InProgress };
        var snapshot = new WhiteboardSnapshot { Id = Guid.NewGuid(), RoomId = room.Id, CanvasDataJson = "old", UpdatedAtUtc = DateTimeOffset.UtcNow.AddDays(-1) };
        fixture.SwapRequests.FindAsync(Arg.Any<System.Linq.Expressions.Expression<Func<SkillSwapAPI.Domain.Modules.SwapRequests.Entities.SwapRequest, bool>>>(), Arg.Any<CancellationToken>()).Returns(swap);
        fixture.LiveSessionRooms.GetBySwapRequestIdAsync(swap.Id, Arg.Any<CancellationToken>()).Returns(room);
        fixture.WhiteboardSnapshots.GetByRoomIdAsync(room.Id, Arg.Any<CancellationToken>()).Returns(snapshot);

        var result = await new SaveWhiteboardSnapshotCommandHandler(fixture.UnitOfWork)
            .Handle(new SaveWhiteboardSnapshotCommand(swap.Id, swap.ReceiverId, "new canvas"), CancellationToken.None);

        Assert.Equal("new canvas", snapshot.CanvasDataJson);
        Assert.Equal("new canvas", result.Value.CanvasDataJson);
        fixture.WhiteboardSnapshots.Received(1).Update(snapshot);
        await fixture.WhiteboardSnapshots.DidNotReceive().AddAsync(Arg.Any<WhiteboardSnapshot>(), Arg.Any<CancellationToken>());
    }
}
