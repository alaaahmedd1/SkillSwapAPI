namespace SkillSwapAPI.Application.Features.Identity.Dtos;

public sealed record UserDto(
    string Id,
    string Email,
    string? FullName);
