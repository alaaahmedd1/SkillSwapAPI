
using SkillSwapAPI.Domain.Common;
using SkillSwapAPI.Domain.Common.Results;

namespace SkillSwapAPI.Domain.Identity;

public sealed class RefreshToken : AuditableEntity
{
    public string? Token { get; }
    public string? UserId { get; }
    public DateTimeOffset ExpiresOnUtc { get; }
    public bool IsRevoked { get; set; }

    private RefreshToken()
    { }

    private RefreshToken(Guid id, string? token, string? userId, DateTimeOffset expiresOnUtc)
        : base(id)
    {
        Token = token;
        UserId = userId;
        ExpiresOnUtc = expiresOnUtc;
        IsRevoked = false;
    }

    public static Result<RefreshToken> Create(Guid id, string? token, string? userId, DateTimeOffset expiresOnUtc)
    {
        if (id == Guid.Empty)
        {
            return RefreshTokenErrors.IdRequired;
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            return RefreshTokenErrors.TokenRequired;
        }

        if (string.IsNullOrWhiteSpace(userId))
        {
            return RefreshTokenErrors.UserIdRequired;
        }

        if (expiresOnUtc <= DateTimeOffset.UtcNow)
        {
            return RefreshTokenErrors.ExpiryInvalid;
        }

        return new RefreshToken(id, token, userId, expiresOnUtc);
    }
}
