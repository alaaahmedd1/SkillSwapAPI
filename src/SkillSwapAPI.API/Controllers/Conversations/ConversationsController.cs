using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSwapAPI.Application.Features.Chat.Queries.GetChatHistory;

namespace SkillSwapAPI.API.Controllers.Conversations;

[Authorize]
[Route("api/v1/conversations")]
public sealed class ConversationsController : ApiBaseController
{
    [HttpGet("{conversationId:guid}/messages")]
    public async Task<IActionResult> GetChatHistory(Guid conversationId, [FromQuery] GetChatHistoryRequest request, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        return HandleResult(await Mediator.Send(
            new GetChatHistoryQuery(conversationId, userId, request.PageNumber, request.PageSize), ct));
    }
}

public sealed record GetChatHistoryRequest(
    int PageNumber = 1,
    int PageSize = 10);
