using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSwapAPI.Application.Features.Payments.Commands.InitiateCheckout;
using SkillSwapAPI.Application.Features.Payments.Queries.GetCreditPackages;

namespace SkillSwapAPI.API.Controllers.Payments;

[Authorize]
[ApiController]
[Route("api/v1/payments")]
public sealed class PaymentsController(ISender sender) : ApiBaseController
{
    [HttpGet("packages")]
    public async Task<IActionResult> GetPackages(CancellationToken ct)
    {
        var result = await sender.Send(new GetCreditPackagesQuery(), ct);
        return Ok(result);
    }

    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout([FromBody] InitiateCheckoutCommand command, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }
        var request = command with { UserId = userId };

        var result = await sender.Send(request, ct);
        return Ok(result);
    }
}