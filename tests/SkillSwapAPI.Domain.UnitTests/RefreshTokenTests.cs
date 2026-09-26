using SkillSwapAPI.Domain.Identity;
using Xunit;

namespace SkillSwapAPI.Domain.UnitTests;

public class RefreshTokenTests
{
    [Fact]
    public void Create_ShouldSucceed_WhenValidDataProvided()
    {
        // Arrange
        var id = Guid.NewGuid();
        var token = "sample-refresh-token-123";
        var userId = "user-guid-id";
        var expiry = DateTimeOffset.UtcNow.AddDays(7);

        // Act
        var result = RefreshToken.Create(id, token, userId, expiry);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(id, result.Value.Id);
        Assert.Equal(token, result.Value.Token);
        Assert.Equal(userId, result.Value.UserId);
        Assert.Equal(expiry, result.Value.ExpiresOnUtc);
        Assert.False(result.Value.IsRevoked);
    }

    [Fact]
    public void Create_ShouldFail_WhenTokenIsEmpty()
    {
        // Arrange
        var id = Guid.NewGuid();
        var token = "";
        var userId = "user-guid-id";
        var expiry = DateTimeOffset.UtcNow.AddDays(7);

        // Act
        var result = RefreshToken.Create(id, token, userId, expiry);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal("RefreshToken_Token_Required", result.TopError.Code);
    }

    [Fact]
    public void Create_ShouldFail_WhenExpiredDateIsInPast()
    {
        // Arrange
        var id = Guid.NewGuid();
        var token = "token-xyz";
        var userId = "user-guid-id";
        var expiry = DateTimeOffset.UtcNow.AddDays(-1);

        // Act
        var result = RefreshToken.Create(id, token, userId, expiry);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal("RefreshToken_Expiry_Invalid", result.TopError.Code);
    }
}
