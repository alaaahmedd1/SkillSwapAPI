using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSwapAPI.Application.Features.Identity.Queries.GetGuestListings;

namespace SkillSwapAPI.API.Controllers.Identity;

[Route("api/[controller]")]
public class GuestFeedController : ApiBaseController
{

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetListings([FromQuery] GetGuestListingsQuery query, CancellationToken ct)
    {
        var result = await Mediator.Send(query, ct);
        return HandleResult(result);
    }
}
