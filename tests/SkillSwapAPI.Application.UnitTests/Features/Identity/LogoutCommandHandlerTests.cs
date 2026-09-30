using Microsoft.Extensions.Logging;
using NSubstitute;
using SkillSwapAPI.Application.Common.Security;
using SkillSwapAPI.Application.Features.Identity.Commands.Logout;
using SkillSwapAPI.Application.UnitTests.Common;
using SkillSwapAPI.Domain.Identity;
using Xunit;

namespace SkillSwapAPI.Application.UnitTests.Features.Identity;

public class LogoutCommandHandlerTests
{
    private const string UserId = "identity-user-id";

    private readonly UnitOfWorkFixture _fixture = new();
    private readonly ILogger<LogoutCommandHandler> _logger = Substitute.For<ILogger<LogoutCommandHandler>>();

    [Fact]
    public async Task Handle_ShouldRevokeRefreshToken_WhenActiveTokenExists()
    {
        var command = new LogoutCommand("refresh-token-value", UserId);
        var tokenHash = RefreshTokenHasher.Hash(command.RefreshToken);
        var refreshToken = RefreshToken.Create(Guid.NewGuid(), tokenHash, UserId, DateTimeOffset.UtcNow.AddDays(7)).Value;

        _fixture.RefreshTokens
            .GetActiveByUserAndTokenAsync(UserId, tokenHash, Arg.Any<CancellationToken>())
            .Returns(refreshToken);

        var handler = new LogoutCommandHandler(_fixture.UnitOfWork, _logger);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsError);
        Assert.True(refreshToken.IsRevoked);
        _fixture.RefreshTokens.Received(1).Update(refreshToken);
        await _fixture.UnitOfWork.Received(1).CompleteAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldLookupTokenByHashedValue_WhenLoggingOut()
    {
        var command = new LogoutCommand("refresh-token-value", UserId);
        var expectedHash = RefreshTokenHasher.Hash("refresh-token-value");

        _fixture.RefreshTokens
            .GetActiveByUserAndTokenAsync(UserId, expectedHash, Arg.Any<CancellationToken>())
            .Returns((RefreshToken?)null);

        var handler = new LogoutCommandHandler(_fixture.UnitOfWork, _logger);

        await handler.Handle(command, CancellationToken.None);

        await _fixture.RefreshTokens.Received(1).GetActiveByUserAndTokenAsync(
            UserId, expectedHash, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WithoutSideEffects_WhenTokenNotFound()
    {
        var command = new LogoutCommand("refresh-token-value", UserId);

        _fixture.RefreshTokens
            .GetActiveByUserAndTokenAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((RefreshToken?)null);

        var handler = new LogoutCommandHandler(_fixture.UnitOfWork, _logger);

        var result = await handler.Handle(command, CancellationToken.None);

        Assert.False(result.IsError);
        _fixture.RefreshTokens.DidNotReceiveWithAnyArgs().Update(default!);
        await _fixture.UnitOfWork.DidNotReceiveWithAnyArgs().CompleteAsync(default);
    }
}
