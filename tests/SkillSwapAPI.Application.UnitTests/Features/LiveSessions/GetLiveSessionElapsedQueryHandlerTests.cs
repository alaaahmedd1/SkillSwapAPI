using NSubstitute;
using SkillSwapAPI.Application.Features.LiveSessions.Queries.GetLiveSessionElapsed;
using SkillSwapAPI.Application.UnitTests.Common;
using SkillSwapAPI.Domain.Modules.LiveSessions.Entities;
using SkillSwapAPI.Domain.Modules.LiveSessions.Enums;
using SkillSwapAPI.Domain.Modules.SwapRequests.Enums;
using Xunit;

namespace SkillSwapAPI.Application.UnitTests.Features.LiveSessions;

public sealed class GetLiveSessionElapsedQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenSessionHasStarted_ComputesElapsedUsingServerStartTime()
    {
        var fixture = new UnitOfWorkFixture();
        var swap = TestData.SwapRequest(status: SwapRequestStatus.Accepted);
        var start = DateTimeOffset.UtcNow.AddSeconds(-12);
        fixture.SwapRequests.FindAsync(Arg.Any<System.Linq.Expressions.Expression<Func<SkillSwapAPI.Domain.Modules.SwapRequests.Entities.SwapRequest, bool>>>(), Arg.Any<CancellationToken>()).Returns(swap);
        fixture.LiveSessionRooms.GetBySwapRequestIdAsync(swap.Id, Arg.Any<CancellationToken>())
            .Returns(new LiveSessionRoom { Id = Guid.NewGuid(), SwapRequestId = swap.Id, RoomToken = "room", Status = LiveSessionStatus.InProgress, ActualStartTime = start });

        var result = await new GetLiveSessionElapsedQueryHandler(fixture.UnitOfWork)
            .Handle(new GetLiveSessionElapsedQuery(swap.Id, swap.ReceiverId), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(start, result.Value.StartedAtUtc);
        Assert.InRange(result.Value.ElapsedSeconds, 11, 16);
    }
}
