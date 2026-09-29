using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSwapAPI.Application.Features.Badges.Queries.GetUserBadges;
using SkillSwapAPI.Application.Features.Reviews.Queries.GetUserReviews;
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

    [HttpGet("{userId:guid}/reviews")]
    public async Task<IActionResult> GetUserReviews(Guid userId, [FromQuery] GetUserReviewsRequest request, CancellationToken ct)
    {
        return HandleResult(await Mediator.Send(
            new GetUserReviewsQuery(userId, request.PageNumber, request.PageSize), ct));
    }

    [HttpGet("{userId:guid}/badges")]
    public async Task<IActionResult> GetUserBadges(Guid userId, CancellationToken ct)
    {
        return HandleResult(await Mediator.Send(new GetUserBadgesQuery(userId), ct));
    }
}

public sealed record GetUserReviewsRequest(
    int PageNumber = 1,
    int PageSize = 10);
