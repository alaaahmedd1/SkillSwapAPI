using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSwapAPI.Application.Features.Skills.Queries.GetSkillCatalog;

namespace SkillSwapAPI.API.Controllers.Skills;

[Route("api/v1/skills")]
public sealed class SkillsController : ApiBaseController
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetCatalog(CancellationToken ct)
    {
        var result = await Mediator.Send(new GetSkillCatalogQuery(), ct);
        return HandleResult(result);
    }
}
