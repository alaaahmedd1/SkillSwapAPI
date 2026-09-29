using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSwapAPI.Application.Features.Badges.Queries.GetBadges;

namespace SkillSwapAPI.API.Controllers.Badges;

[Authorize]
[Route("api/v1/badges")]
public sealed class BadgesController : ApiBaseController
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetBadges(CancellationToken ct)
    {
        return HandleResult(await Mediator.Send(new GetBadgesQuery(), ct));
    }
}
