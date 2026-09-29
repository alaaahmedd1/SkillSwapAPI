using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSwapAPI.Application.Features.Reviews.Commands.SubmitReview;

namespace SkillSwapAPI.API.Controllers.Reviews;

[Authorize]
[Route("api/v1/reviews")]
public sealed class ReviewsController : ApiBaseController
{
    [HttpPost]
    public async Task<IActionResult> SubmitReview([FromBody] SubmitReviewRequest request, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        var result = await Mediator.Send(
            new SubmitReviewCommand(
                request.SwapRequestId,
                userId,
                request.RevieweeId,
                request.Rating,
                request.Comment,
                request.BadgeId), ct);

        if (result.IsError)
        {
            return HandleResult(result);
        }

        return StatusCode(StatusCodes.Status201Created, result.Value);
    }
}

public sealed record SubmitReviewRequest(
    Guid SwapRequestId,
    Guid RevieweeId,
    int Rating,
    string? Comment,
    int? BadgeId);
