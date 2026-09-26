namespace SkillSwapAPI.Application.Features.Identity;

public sealed record TokenResponse(string AccessToken, string RefreshToken, DateTime ExpiresOnUtc);