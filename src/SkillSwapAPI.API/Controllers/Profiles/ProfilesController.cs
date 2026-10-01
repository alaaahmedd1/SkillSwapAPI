using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSwapAPI.Application.Features.Users.Commands.AddUserSkill;
using SkillSwapAPI.Application.Features.Users.Commands.RemoveUserSkill;
using SkillSwapAPI.Application.Features.Users.Commands.UpdateMyAvailability;
using SkillSwapAPI.Application.Features.Users.Commands.UpdateProfile;
using SkillSwapAPI.Application.Features.Users.Dtos;
using SkillSwapAPI.Application.Features.Users.Queries.GetMyAvailability;
using SkillSwapAPI.Application.Features.Users.Queries.GetOwnProfile;
using SkillSwapAPI.Domain.Modules.Users.Enums;

namespace SkillSwapAPI.API.Controllers.Profiles;

[Authorize]
[Route("api/v1/profiles/me")]
public sealed class ProfilesController : ApiBaseController
{
    [HttpGet]
    public async Task<IActionResult> GetOwnProfile(CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        return HandleResult(await Mediator.Send(new GetOwnProfileQuery(userId), ct));
    }

    [HttpPut]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        return HandleResult(await Mediator.Send(
            new UpdateProfileCommand(
                userId,
                request.FirstName,
                request.LastName,
                request.Title,
                request.Bio,
                request.City,
                request.Country,
                request.TimeZone,
                request.OpenForInstantSwaps,
                request.OnlineOnly,
                request.AutoMatchBarterRequests), ct));
    }

    [HttpGet("availability")]
    public async Task<IActionResult> GetAvailability(CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        return HandleResult(await Mediator.Send(new GetMyAvailabilityQuery(userId), ct));
    }

    [HttpPut("availability")]
    public async Task<IActionResult> UpdateAvailability(
        [FromBody] UpdateAvailabilityRequest request,
        CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        return HandleResult(await Mediator.Send(
            new UpdateMyAvailabilityCommand(userId, request.Slots), ct));
    }

    [HttpPost("skills")]
    public async Task<IActionResult> AddSkill([FromBody] AddUserSkillRequest request, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        return HandleResult(await Mediator.Send(
            new AddUserSkillCommand(
                userId,
                request.SkillId,
                request.Type,
                request.ProficiencyLevel,
                request.YearsOfExperience), ct));
    }

    [HttpDelete("skills/{userSkillId:guid}")]
    public async Task<IActionResult> RemoveSkill(Guid userSkillId, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        return HandleResult(await Mediator.Send(new RemoveUserSkillCommand(userId, userSkillId), ct));
    }
}

public sealed record UpdateProfileRequest(
    string FirstName,
    string LastName,
    string? Title = null,
    string? Bio = null,
    string? City = null,
    string? Country = null,
    string? TimeZone = null,
    bool? OpenForInstantSwaps = null,
    bool? OnlineOnly = null,
    bool? AutoMatchBarterRequests = null);

public sealed record UpdateAvailabilityRequest(IReadOnlyList<AvailabilitySlotDto> Slots);

public sealed record AddUserSkillRequest(
    Guid SkillId,
    SkillType Type,
    ProficiencyLevel ProficiencyLevel,
    int? YearsOfExperience);
