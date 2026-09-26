using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSwapAPI.Application.Features.Users.Queries.SearchUsers;

namespace SkillSwapAPI.API.Controllers.Users;

[Authorize]
[Route("api/v1/users")]
public sealed class UsersController : ApiBaseController
{
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] SearchUsersQuery query, CancellationToken ct)
    {
        return HandleResult(await Mediator.Send(query, ct));
    }
}
