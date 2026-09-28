using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SkillSwapAPI.Application.Common.Interfaces.Identity;
using SkillSwapAPI.Application.Common.Interfaces.UnitOfWork;
using SkillSwapAPI.Application.Common.Security;
using SkillSwapAPI.Application.Features.Identity;
using SkillSwapAPI.Application.Features.Identity.Dtos;
using SkillSwapAPI.Domain.Common.Results;
using SkillSwapAPI.Domain.Identity;
using SkillSwapAPI.Infrastructure.Settings;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SkillSwapAPI.Infrastructure.Identity;

public sealed class TokenProvider(
    IOptions<JwtSettings> jwtOptions,
    IUnitOfWork unitOfWork)
    : ITokenProvider
{
    private readonly JwtSettings _jwtSettings = jwtOptions.Value;

    public async Task<Result<TokenResponse>> GenerateJwtTokenAsync(
        AppUserDto user,
        CancellationToken ct = default)
    {
        var expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        foreach (var role in user.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        foreach (var claim in user.Claims)
        {
            if (claims.Any(existing => existing.Type == claim.Type && existing.Value == claim.Value))
            {
                continue;
            }

            claims.Add(claim);
        }

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expires,
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey)),
                SecurityAlgorithms.HmacSha256Signature),
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var securityToken = tokenHandler.CreateToken(descriptor);
        var accessToken = tokenHandler.WriteToken(securityToken);

        var refreshToken = GenerateRefreshToken();

        var refreshTokenResult = RefreshToken.Create(
            Guid.NewGuid(),
            RefreshTokenHasher.Hash(refreshToken),
            user.UserId.ToString(),
            DateTimeOffset.UtcNow.AddDays(_jwtSettings.RefreshExpiryDays));

        if (!refreshTokenResult.IsSuccess)
        {
            return Error.Unexpected(
                "Token.GenerationFailed",
                "An error occurred while generating the token.");
        }

        await unitOfWork.RefreshTokens.AddAsync(refreshTokenResult.Value, ct);
        await unitOfWork.CompleteAsync(ct);

        return new TokenResponse(
            accessToken,
            refreshToken,
            expires
        );
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string expiredAccessToken)
    {
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

        var parameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwtSettings.SecretKey)),
            ValidateIssuer = true,
            ValidIssuer = _jwtSettings.Issuer,
            ValidateAudience = true,
            ValidAudience = _jwtSettings.Audience,
            ValidateLifetime = false,
            ClockSkew = TimeSpan.Zero,
            NameClaimType = ClaimTypes.NameIdentifier,
        };

        try
        {
            var principal = new JwtSecurityTokenHandler()
                .ValidateToken(expiredAccessToken, parameters, out var securityToken);

            if (securityToken is not JwtSecurityToken jwt ||
                !jwt.Header.Alg.Equals(
                    SecurityAlgorithms.HmacSha256,
                    StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return principal;
        }
        catch (Exception ex)
        {
            Console.WriteLine("💥 Token Validation Failed: " + ex.Message);
            return null;
        }
    }

    private static string GenerateRefreshToken() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
}
