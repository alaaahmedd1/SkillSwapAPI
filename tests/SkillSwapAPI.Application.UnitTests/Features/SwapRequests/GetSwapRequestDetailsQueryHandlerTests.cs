using NSubstitute;
using SkillSwapAPI.Application.Common.Interfaces.Identity;
using SkillSwapAPI.Application.Features.SwapRequests.Queries.GetSwapRequestDetails;
using SkillSwapAPI.Application.UnitTests.Common;
using Xunit;

namespace SkillSwapAPI.Application.UnitTests.Features.SwapRequests;

public sealed class GetSwapRequestDetailsQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenCallerIsNotParticipant_DeniesDetailsWithoutLoadingProfiles()
    {
        var fixture = new UnitOfWorkFixture();
        var identity = Substitute.For<IIdentityService>();
        var swap = TestData.SwapRequest();
        fixture.SwapRequests.GetByIdWithDetailsAsync(swap.Id, Arg.Any<CancellationToken>()).Returns(swap);

        var result = await new GetSwapRequestDetailsQueryHandler(fixture.UnitOfWork, identity)
            .Handle(new GetSwapRequestDetailsQuery(swap.Id, Guid.NewGuid()), CancellationToken.None);

        Assert.False(result.IsSuccess);
        await identity.DidNotReceive().GetProfilesAsync(Arg.Any<IReadOnlyCollection<Guid>>(), Arg.Any<CancellationToken>());
    }
}
