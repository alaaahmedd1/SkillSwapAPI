using Google.Apis.Auth;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SkillSwapAPI.Application.Common.Errors;
using SkillSwapAPI.Application.Common.Interfaces.Notifications;
using SkillSwapAPI.Application.Common.Models;
using SkillSwapAPI.Domain.Common.Results;
using SkillSwapAPI.Domain.Identity;
using SkillSwapAPI.Infrastructure.Settings;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json.Serialization;

namespace SkillSwapAPI.Infrastructure.Identity;

public sealed class SocialAuthService(
    IOptions<SocialAuthSettings> options,
    IHttpClientFactory httpClientFactory,
    ILogger<SocialAuthService> logger)
    : ISocialAuthService
{
    private readonly SocialAuthSettings _settings = options.Value;

    public async Task<Result<SocialUserInfo>> VerifyTokenAsync(         
        string idToken,
        SocialProvider provider,
        CancellationToken ct = default)
    {
        return provider switch
        {
            SocialProvider.Google => await VerifyGoogleAsync(idToken, ct),
            SocialProvider.Facebook => await VerifyFacebookAsync(idToken, ct),
            SocialProvider.Apple => await VerifyAppleAsync(idToken, ct),
            _ => ApplicationErrors.SocialAuth.ProviderNotSupported,
        };
    }

    private async Task<Result<SocialUserInfo>> VerifyGoogleAsync(
        string idToken, CancellationToken ct)
    {
        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(
                idToken,
                new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = [_settings.Google.ClientId],
                });

            if (string.IsNullOrWhiteSpace(payload.Email))
            {
                logger.LogWarning("Google token valid but email missing");
                return ApplicationErrors.SocialAuth.EmailNotProvided;
            }

            return new SocialUserInfo(
                ProviderUserId: payload.Subject,
                Email: payload.Email,
                FullName: payload.Name ?? payload.Email,
                Provider: "Google");
        }
        catch (InvalidJwtException ex)
        {
            logger.LogWarning(ex, "Invalid Google JWT");
            return ApplicationErrors.SocialAuth.InvalidToken;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error verifying Google token");
            return ApplicationErrors.SocialAuth.TokenVerificationFailed;
        }
    }

    private async Task<Result<SocialUserInfo>> VerifyFacebookAsync(
        string accessToken, CancellationToken ct)
    {
        try
        {
            var client = httpClientFactory.CreateClient("Facebook");
            var appToken = $"{_settings.Facebook.AppId}|{_settings.Facebook.AppSecret}";

            var debug = await client.GetFromJsonAsync<FacebookDebugResponse>(
                $"debug_token?input_token={accessToken}&access_token={appToken}", ct);

            if (debug?.Data is null || !debug.Data.IsValid)
            {
                logger.LogWarning("Facebook token is invalid");
                return ApplicationErrors.SocialAuth.InvalidToken;
            }

            if (debug.Data.AppId != _settings.Facebook.AppId)
            {
                logger.LogWarning("Facebook token belongs to different app");
                return ApplicationErrors.SocialAuth.InvalidToken;
            }

            var fbUser = await client.GetFromJsonAsync<FacebookUserResponse>(
                $"me?fields=id,name,email&access_token={accessToken}", ct);

            if (fbUser is null)
                return ApplicationErrors.SocialAuth.TokenVerificationFailed;

            if (string.IsNullOrWhiteSpace(fbUser.Email))
            {
                logger.LogWarning("Facebook user {Id} has no email", fbUser.Id);
                return ApplicationErrors.SocialAuth.EmailNotProvided;
            }

            return new SocialUserInfo(
                ProviderUserId: fbUser.Id,
                Email: fbUser.Email,
                FullName: fbUser.Name ?? fbUser.Email,
                Provider: "Facebook");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error verifying Facebook token");
            return ApplicationErrors.SocialAuth.TokenVerificationFailed;
        }
    }

    private async Task<Result<SocialUserInfo>> VerifyAppleAsync(string idToken, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(idToken))
            return ApplicationErrors.SocialAuth.InvalidToken;

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(idToken);
            var kid = jwt.Header.Kid;

            var client = httpClientFactory.CreateClient("Apple");
            var jwks = await client.GetFromJsonAsync<AppleJwksResponse>(
                "https://appleid.apple.com/auth/keys", ct);

            var matchingKey = jwks?.Keys.FirstOrDefault(k => k.Kid == kid);
            if (matchingKey is null)
            {
                logger.LogWarning("No matching Apple JWK found for kid {Kid}", kid);
                return ApplicationErrors.SocialAuth.InvalidToken;
            }

            var rsa = RSA.Create();
            rsa.ImportParameters(new RSAParameters
            {
                Modulus = Base64UrlEncoder.DecodeBytes(matchingKey.N),
                Exponent = Base64UrlEncoder.DecodeBytes(matchingKey.E)
            });

            var validationParams = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = "https://appleid.apple.com",
                ValidateAudience = true,
                ValidAudience = _settings.Apple.ClientId,
                ValidateLifetime = true,
                IssuerSigningKey = new RsaSecurityKey(rsa),
                ValidateIssuerSigningKey = true
            };

            var principal = handler.ValidateToken(idToken, validationParams, out _);

            var sub = principal.FindFirst("sub")?.Value;
            var email = principal.FindFirst("email")?.Value;

            if (string.IsNullOrWhiteSpace(sub))
                return ApplicationErrors.SocialAuth.InvalidToken;

            return new SocialUserInfo(
                ProviderUserId: sub,
                Email: email ?? string.Empty,
                FullName: email ?? "Apple User",
                Provider: "Apple");
        }
        catch (SecurityTokenException ex)
        {
            logger.LogWarning(ex, "Invalid Apple JWT");
            return ApplicationErrors.SocialAuth.InvalidToken;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error verifying Apple token");
            return ApplicationErrors.SocialAuth.TokenVerificationFailed;
        }
    }

    private sealed class AppleJwksResponse
    {
        public List<AppleJwk> Keys { get; init; } = [];
    }

    private sealed class AppleJwk
    {
        public string Kid { get; init; } = string.Empty;
        public string N { get; init; } = string.Empty;
        public string E { get; init; } = string.Empty;
    }

    private sealed class FacebookDebugResponse
    {
        public FacebookTokenData? Data { get; init; }
    }

    private sealed class FacebookTokenData
    {
        [JsonPropertyName("app_id")]
        public string AppId { get; init; } = string.Empty;

        [JsonPropertyName("is_valid")]
        public bool IsValid { get; init; }
    }

    private sealed class FacebookUserResponse
    {
        public string Id { get; init; } = string.Empty;
        public string? Name { get; init; }
        public string? Email { get; init; }
    }
}