using NSubstitute;
using SkillSwapAPI.Application.Features.LiveSessions.Queries.GetLiveSessionAccess;
using SkillSwapAPI.Application.UnitTests.Common;
using SkillSwapAPI.Domain.Modules.LiveSessions.Entities;
using SkillSwapAPI.Domain.Modules.LiveSessions.Enums;
using SkillSwapAPI.Domain.Modules.SwapRequests.Enums;
using Xunit;

namespace SkillSwapAPI.Application.UnitTests.Features.LiveSessions;

public sealed class GetLiveSessionAccessQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenAcceptedSwapParticipantHasActiveRoom_AllowsAccess()
    {
        var fixture = new UnitOfWorkFixture();
        var swap = TestData.SwapRequest(status: SwapRequestStatus.Accepted);
        var room = new LiveSessionRoom { Id = Guid.NewGuid(), SwapRequestId = swap.Id, RoomToken = "room", Status = LiveSessionStatus.Waiting };
        fixture.SwapRequests.FindAsync(Arg.Any<System.Linq.Expressions.Expression<Func<SkillSwapAPI.Domain.Modules.SwapRequests.Entities.SwapRequest, bool>>>(), Arg.Any<CancellationToken>()).Returns(swap);
        fixture.LiveSessionRooms.GetBySwapRequestIdAsync(swap.Id, Arg.Any<CancellationToken>()).Returns(room);

        var result = await new GetLiveSessionAccessQueryHandler(fixture.UnitOfWork)
            .Handle(new GetLiveSessionAccessQuery(swap.Id, swap.RequesterId), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Theory]
    [InlineData(LiveSessionStatus.Ended)]
    public async Task Handle_WhenRoomHasEnded_DeniesAccess(LiveSessionStatus status)
    {
        var fixture = new UnitOfWorkFixture();
        var swap = TestData.SwapRequest(status: SwapRequestStatus.Accepted);
        fixture.SwapRequests.FindAsync(Arg.Any<System.Linq.Expressions.Expression<Func<SkillSwapAPI.Domain.Modules.SwapRequests.Entities.SwapRequest, bool>>>(), Arg.Any<CancellationToken>()).Returns(swap);
        fixture.LiveSessionRooms.GetBySwapRequestIdAsync(swap.Id, Arg.Any<CancellationToken>())
            .Returns(new LiveSessionRoom { Id = Guid.NewGuid(), SwapRequestId = swap.Id, RoomToken = "room", Status = status });

        var result = await new GetLiveSessionAccessQueryHandler(fixture.UnitOfWork)
            .Handle(new GetLiveSessionAccessQuery(swap.Id, swap.ReceiverId), CancellationToken.None);

        Assert.False(result.IsSuccess);
    }
}
