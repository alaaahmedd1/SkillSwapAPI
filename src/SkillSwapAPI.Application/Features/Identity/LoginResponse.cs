using SkillSwapAPI.Application.Features.Identity.Dtos;

namespace SkillSwapAPI.Application.Features.Identity;

public sealed record LoginResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresOnUtc,
    UserDto User);
