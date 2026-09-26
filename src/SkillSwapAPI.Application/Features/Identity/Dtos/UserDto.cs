namespace SkillSwapAPI.Application.Features.Identity.Dtos;

public sealed record UserDto(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    string? Token);
