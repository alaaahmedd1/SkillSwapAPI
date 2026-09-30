using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using SkillSwapAPI.Application.Common.Interfaces.UnitOfWork;
using SkillSwapAPI.Application.Common.Security;
using SkillSwapAPI.Application.Features.Identity.Dtos;
using SkillSwapAPI.Domain.Identity;
using SkillSwapAPI.Infrastructure.Identity;
using SkillSwapAPI.Infrastructure.Settings;
using Xunit;

namespace SkillSwapAPI.Infrastructure.UnitTests;

public sealed class TokenProviderTests
{
    private const string Secret = "token-provider-test-secret-at-least-thirty-two-bytes";

    [Fact]
    public async Task GenerateJwtTokenAsync_SignsUserClaimsAndPersistsOnlyHashedRefreshToken()
    {
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var refreshTokens = Substitute.For<SkillSwapAPI.Application.Common.Interfaces.Repos.IRefreshTokenRepository>();
        unitOfWork.RefreshTokens.Returns(refreshTokens);
        unitOfWork.CompleteAsync(Arg.Any<CancellationToken>()).Returns(1);
        RefreshToken? persistedRefreshToken = null;
        refreshTokens.AddAsync(Arg.Do<RefreshToken>(token => persistedRefreshToken = token), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
        var userId = Guid.NewGuid();
        var user = new AppUserDto(userId, "ada@example.com", "Ada", "Lovelace", 5m, 3, true,
            DateTimeOffset.UtcNow, new List<string> { "Admin" }, new List<Claim>());

        var result = await new TokenProvider(Options.Create(Settings()), unitOfWork).GenerateJwtTokenAsync(user);

        Assert.True(result.IsSuccess);
        var token = new JwtSecurityTokenHandler().ReadJwtToken(result.Value.AccessToken);
        Assert.Equal("test-issuer", token.Issuer);
        Assert.Contains(token.Audiences, audience => audience == "test-audience");
        Assert.Contains(token.Claims, claim => (claim.Type == ClaimTypes.NameIdentifier || claim.Type == "nameid") && claim.Value == userId.ToString());
        Assert.Contains(token.Claims, claim => (claim.Type == ClaimTypes.Role || claim.Type == "role") && claim.Value == "Admin");
        Assert.NotNull(persistedRefreshToken);
        Assert.Equal(RefreshTokenHasher.Hash(result.Value.RefreshToken), persistedRefreshToken.Token);
        Assert.NotEqual(result.Value.RefreshToken, persistedRefreshToken.Token);
        await unitOfWork.Received(1).CompleteAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_AcceptsExpiredValidTokenAndRejectsWrongSignature()
    {
        var provider = new TokenProvider(Options.Create(Settings()), Substitute.For<IUnitOfWork>());
        var userId = Guid.NewGuid();
        var expired = CreateToken(Secret, userId, DateTime.UtcNow.AddMinutes(-2));
        var wrongKey = CreateToken("different-signing-key-with-enough-bytes-for-hmac", userId, DateTime.UtcNow.AddMinutes(-2));

        var principal = provider.GetPrincipalFromExpiredToken(expired);

        Assert.Equal(userId.ToString(), principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value);
        Assert.Null(provider.GetPrincipalFromExpiredToken(wrongKey));
    }

    private static JwtSettings Settings() => new()
    {
        SecretKey = Secret,
        Issuer = "test-issuer",
        Audience = "test-audience",
        ExpiryMinutes = 20,
        RefreshExpiryDays = 5
    };

    private static string CreateToken(string secret, Guid userId, DateTime expires)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken("test-issuer", "test-audience",
            [new Claim(ClaimTypes.NameIdentifier, userId.ToString())],
            expires: expires, signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
