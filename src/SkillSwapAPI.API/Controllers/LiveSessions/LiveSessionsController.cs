using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSwapAPI.Application.Features.LiveSessions.Commands.EndLiveSession;
using SkillSwapAPI.Application.Features.LiveSessions.Commands.JoinLiveSession;

namespace SkillSwapAPI.API.Controllers.LiveSessions;

[Authorize]
[Route("api/v1/live-sessions")]
public sealed class LiveSessionsController : ApiBaseController
{
    [HttpPost("{swapId:guid}/join")]
    public async Task<IActionResult> JoinLiveSession(Guid swapId, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        return HandleResult(await Mediator.Send(new JoinLiveSessionCommand(swapId, userId), ct));
    }

    [HttpPost("{roomId:guid}/end")]
    public async Task<IActionResult> EndLiveSession(Guid roomId, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        return HandleResult(await Mediator.Send(new EndLiveSessionCommand(roomId, userId), ct));
    }
}
