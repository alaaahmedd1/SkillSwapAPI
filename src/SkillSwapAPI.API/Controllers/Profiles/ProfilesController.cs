using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSwapAPI.Application.Features.Users.Commands.AddUserSkill;
using SkillSwapAPI.Application.Features.Users.Commands.RemoveUserSkill;
using SkillSwapAPI.Application.Features.Users.Commands.UpdateProfile;
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
            new UpdateProfileCommand(userId, request.FirstName, request.LastName), ct));
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

public sealed record UpdateProfileRequest(string FirstName, string LastName);

public sealed record AddUserSkillRequest(
    Guid SkillId,
    SkillType Type,
    ProficiencyLevel ProficiencyLevel,
    int? YearsOfExperience);
