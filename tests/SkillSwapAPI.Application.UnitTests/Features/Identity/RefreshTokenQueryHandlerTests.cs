using System.Security.Claims;
using Microsoft.Extensions.Logging;
using NSubstitute;
using SkillSwapAPI.Application.Common.Interfaces.Identity;
using SkillSwapAPI.Application.Common.Security;
using SkillSwapAPI.Application.Features.Identity;
using SkillSwapAPI.Application.Features.Identity.Dtos;
using SkillSwapAPI.Application.Features.Identity.Queries.RefreshTokens;
using SkillSwapAPI.Application.UnitTests.Common;
using SkillSwapAPI.Domain.Common.Results;
using SkillSwapAPI.Domain.Identity;
using Xunit;

namespace SkillSwapAPI.Application.UnitTests.Features.Identity;

public sealed class RefreshTokenQueryHandlerTests
{
    [Fact]
    public async Task Handle_WithActiveRefreshToken_RotatesTokenAndReturnsNewJwt()
    {
        var fixture = new UnitOfWorkFixture();
        var tokenProvider = Substitute.For<ITokenProvider>();
        var identity = Substitute.For<IIdentityService>();
        var userId = Guid.NewGuid();
        const string refreshValue = "refresh-secret";
        var expiredToken = "expired-access-token";
        var refreshToken = RefreshToken.Create(Guid.NewGuid(), RefreshTokenHasher.Hash(refreshValue), userId.ToString(), DateTimeOffset.UtcNow.AddDays(1)).Value;
        var user = User(userId);
        var newTokens = new TokenResponse("new-access", "new-refresh", DateTime.UtcNow.AddMinutes(15));
        tokenProvider.GetPrincipalFromExpiredToken(expiredToken).Returns(Principal(userId));
        fixture.RefreshTokens.GetByUserAndTokenAsync(userId.ToString(), RefreshTokenHasher.Hash(refreshValue), Arg.Any<CancellationToken>()).Returns(refreshToken);
        identity.GetUserByIdAsync(userId.ToString(), Arg.Any<CancellationToken>()).Returns(Result<AppUserDto>.FromSuccess(user));
        tokenProvider.GenerateJwtTokenAsync(user, Arg.Any<CancellationToken>()).Returns(Result<TokenResponse>.FromSuccess(newTokens));

        var result = await CreateHandler(fixture, tokenProvider, identity).Handle(new RefreshTokenQuery(refreshValue, expiredToken), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(newTokens, result.Value);
        Assert.True(refreshToken.IsRevoked);
        fixture.RefreshTokens.Received(1).Update(refreshToken);
        await fixture.UnitOfWork.Received(1).CompleteAsync(Arg.Any<CancellationToken>());
        await tokenProvider.Received(1).GenerateJwtTokenAsync(user, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenRevokedRefreshTokenIsReused_RevokesAllOtherActiveTokens()
    {
        var fixture = new UnitOfWorkFixture();
        var tokenProvider = Substitute.For<ITokenProvider>();
        var identity = Substitute.For<IIdentityService>();
        var userId = Guid.NewGuid();
        const string refreshValue = "replayed-secret";
        var replayed = RefreshToken.Create(Guid.NewGuid(), RefreshTokenHasher.Hash(refreshValue), userId.ToString(), DateTimeOffset.UtcNow.AddDays(1)).Value;
        replayed.IsRevoked = true;
        var stillActive = RefreshToken.Create(Guid.NewGuid(), "other-hash", userId.ToString(), DateTimeOffset.UtcNow.AddDays(1)).Value;
        tokenProvider.GetPrincipalFromExpiredToken(Arg.Any<string>()).Returns(Principal(userId));
        fixture.RefreshTokens.GetByUserAndTokenAsync(userId.ToString(), RefreshTokenHasher.Hash(refreshValue), Arg.Any<CancellationToken>()).Returns(replayed);
        fixture.RefreshTokens.GetActiveByUserAsync(userId.ToString(), Arg.Any<CancellationToken>()).Returns([stillActive]);

        var result = await CreateHandler(fixture, tokenProvider, identity).Handle(new RefreshTokenQuery(refreshValue, "expired-token"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.True(stillActive.IsRevoked);
        fixture.RefreshTokens.Received(1).UpdateRange(Arg.Is<IEnumerable<RefreshToken>>(items => items.Single() == stillActive));
        await fixture.UnitOfWork.Received(1).CompleteAsync(Arg.Any<CancellationToken>());
        await identity.DidNotReceive().GetUserByIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenExpiredAccessTokenHasNoValidPrincipal_ReturnsErrorWithoutLookingUpRefreshToken()
    {
        var fixture = new UnitOfWorkFixture();
        var tokenProvider = Substitute.For<ITokenProvider>();
        tokenProvider.GetPrincipalFromExpiredToken("tampered-token").Returns((ClaimsPrincipal?)null);

        var result = await CreateHandler(fixture, tokenProvider, Substitute.For<IIdentityService>())
            .Handle(new RefreshTokenQuery("refresh", "tampered-token"), CancellationToken.None);

        Assert.False(result.IsSuccess);
        await fixture.RefreshTokens.DidNotReceive().GetByUserAndTokenAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        await fixture.UnitOfWork.DidNotReceive().CompleteAsync(Arg.Any<CancellationToken>());
    }

    private static RefreshTokenQueryHandler CreateHandler(UnitOfWorkFixture fixture, ITokenProvider provider, IIdentityService identity) =>
        new(provider, identity, fixture.UnitOfWork, Substitute.For<ILogger<RefreshTokenQueryHandler>>());

    private static ClaimsPrincipal Principal(Guid userId) => new(new ClaimsIdentity(
        [new Claim(ClaimTypes.NameIdentifier, userId.ToString())], "Bearer"));

    private static AppUserDto User(Guid userId) => new(userId, "person@example.com", "Test", "User", 4.5m, 2, true,
        DateTimeOffset.UtcNow, new List<string> { "User" }, new List<Claim>());
}
