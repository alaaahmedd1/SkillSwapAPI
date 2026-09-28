using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSwapAPI.Application.Features.Admin.Commands.CreateCategory;
using SkillSwapAPI.Application.Features.Admin.Commands.CreateSkill;
using SkillSwapAPI.Application.Features.Admin.Commands.UpdateUserStatus;
using SkillSwapAPI.Application.Features.Admin.Queries.GetUsers;

namespace SkillSwapAPI.API.Controllers.Admin;

[Authorize(Roles = "Admin")]
[Route("api/v1/admin")]
public sealed class AdminController : ApiBaseController
{
    [HttpPost("categories")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        var result = await Mediator.Send(
            new CreateCategoryCommand(userId, request.Name, request.Description), ct);

        if (result.IsError)
        {
            return HandleResult(result);
        }

        return StatusCode(StatusCodes.Status201Created, result.Value);
    }

    [HttpPost("skills")]
    public async Task<IActionResult> CreateSkill([FromBody] CreateSkillRequest request, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        var result = await Mediator.Send(
            new CreateSkillCommand(userId, request.CategoryId, request.Name, request.Description), ct);

        if (result.IsError)
        {
            return HandleResult(result);
        }

        return StatusCode(StatusCodes.Status201Created, result.Value);
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers([FromQuery] GetUsersRequest request, CancellationToken ct)
    {
        return HandleResult(await Mediator.Send(
            new GetUsersQuery(request.IsActive, request.SearchTerm, request.PageNumber, request.PageSize), ct));
    }

    [HttpPut("users/{userId:guid}/status")]
    public async Task<IActionResult> UpdateUserStatus(Guid userId, [FromBody] UpdateUserStatusRequest request, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var adminId))
        {
            return Unauthorized();
        }

        return HandleResult(await Mediator.Send(
            new UpdateUserStatusCommand(adminId, userId, request.IsActive), ct));
    }
}

public sealed record CreateCategoryRequest(
    string Name,
    string? Description);

public sealed record CreateSkillRequest(
    int CategoryId,
    string Name,
    string? Description);

public sealed record GetUsersRequest(
    bool? IsActive = null,
    string? SearchTerm = null,
    int PageNumber = 1,
    int PageSize = 10);

public sealed record UpdateUserStatusRequest(bool IsActive);
