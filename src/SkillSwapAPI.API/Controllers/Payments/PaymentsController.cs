using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSwapAPI.Application.Features.Payments.Commands.InitiateCheckout;
using SkillSwapAPI.Application.Features.Payments.Commands.ProcessPaymentWebhook;
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
        var result = await sender.Send(command, ct);
        return Ok(result);
    }

    /// <summary>
    /// Handles Stripe => validates signature and issuance
    /// </summary>
    [AllowAnonymous]
    [HttpPost("webhook")]
    public async Task<IActionResult> StripeWebhook(CancellationToken ct)
    {
        var jsonPayload = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync(ct);
        var stripeSignature = Request.Headers["Stripe-Signature"].ToString();

        try
        {
            await sender.Send(new ProcessPaymentWebhookCommand(jsonPayload, stripeSignature), ct);
            return Ok();
        }
        catch (Exception)
        {
            return BadRequest();
        }
    }
}