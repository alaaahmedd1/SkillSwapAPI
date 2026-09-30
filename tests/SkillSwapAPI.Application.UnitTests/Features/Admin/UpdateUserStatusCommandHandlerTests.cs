using NSubstitute;
using SkillSwapAPI.Application.Common.Interfaces.Identity;
using SkillSwapAPI.Application.Common.Interfaces.Services;
using SkillSwapAPI.Application.Features.Admin.Commands.UpdateUserStatus;
using SkillSwapAPI.Application.Features.Users.Dtos;
using SkillSwapAPI.Application.UnitTests.Common;
using SkillSwapAPI.Domain.Common.Results;
using SkillSwapAPI.Domain.Identity;
using SkillSwapAPI.Domain.Modules.Administration.Entities;
using SkillSwapAPI.Domain.Modules.SwapRequests.Enums;
using Xunit;

namespace SkillSwapAPI.Application.UnitTests.Features.Admin;

public sealed class UpdateUserStatusCommandHandlerTests
{
    [Fact]
    public async Task SuspendUser_RevokesTokensCancelsActiveSwapsAuditsAndDisconnectsUser()
    {
        var fixture = new UnitOfWorkFixture();
        var identity = Substitute.For<IIdentityService>();
        var connections = Substitute.For<IChatConnectionManager>();
        var adminId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var token = RefreshToken.Create(Guid.NewGuid(), "hashed-token", userId.ToString(), DateTimeOffset.UtcNow.AddDays(1)).Value;
        var swap = TestData.SwapRequest(requesterId: userId, status: SwapRequestStatus.Accepted);
        fixture.RefreshTokens.GetActiveByUserAsync(userId.ToString(), Arg.Any<CancellationToken>()).Returns([token]);
        fixture.SwapRequests.GetActiveByUserAsync(userId, Arg.Any<CancellationToken>()).Returns([swap]);
        identity.UpdateUserStatusAsync(userId, false, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<ProfileIdentityDto>.FromSuccess(Profile(userId, false))));
        AuditLog? audit = null;
        fixture.AuditLogs.AddAsync(Arg.Do<AuditLog>(entry => audit = entry), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        var result = await new UpdateUserStatusCommandHandler(fixture.UnitOfWork, identity, connections)
            .Handle(new UpdateUserStatusCommand(adminId, userId, false), CancellationToken.None);

        Assert.True(token.IsRevoked);
        Assert.Equal(SwapRequestStatus.Cancelled, swap.Status);
        Assert.Equal("BanUser", audit?.Action);
        Assert.Equal(adminId, audit?.AdminId);
        Assert.Equal(userId.ToString(), audit?.TargetEntityId);
        Assert.Equal(userId, result.Value.UserId);
        fixture.RefreshTokens.Received(1).UpdateRange(Arg.Is<IEnumerable<RefreshToken>>(items => items.Single() == token));
        fixture.SwapRequests.Received(1).UpdateRange(Arg.Is<IEnumerable<SkillSwapAPI.Domain.Modules.SwapRequests.Entities.SwapRequest>>(items => items.Single() == swap));
        await fixture.UnitOfWork.Received(1).CompleteAsync(Arg.Any<CancellationToken>());
        await connections.Received(1).DisconnectUserAsync(userId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ActivateUser_WritesUnbanAuditWithoutRevokingOrDisconnecting()
    {
        var fixture = new UnitOfWorkFixture();
        var identity = Substitute.For<IIdentityService>();
        var connections = Substitute.For<IChatConnectionManager>();
        var userId = Guid.NewGuid();
        identity.UpdateUserStatusAsync(userId, true, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<ProfileIdentityDto>.FromSuccess(Profile(userId, true))));
        AuditLog? audit = null;
        fixture.AuditLogs.AddAsync(Arg.Do<AuditLog>(entry => audit = entry), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        await new UpdateUserStatusCommandHandler(fixture.UnitOfWork, identity, connections)
            .Handle(new UpdateUserStatusCommand(Guid.NewGuid(), userId, true), CancellationToken.None);

        Assert.Equal("UnbanUser", audit?.Action);
        await fixture.RefreshTokens.DidNotReceive().GetActiveByUserAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await fixture.SwapRequests.DidNotReceive().GetActiveByUserAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await connections.DidNotReceive().DisconnectUserAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await fixture.UnitOfWork.Received(1).CompleteAsync(Arg.Any<CancellationToken>());
    }

    private static ProfileIdentityDto Profile(Guid id, bool active) =>
        new(id, "Test", "User", "test@example.com", 0, 0, active, DateTimeOffset.UtcNow);
}
