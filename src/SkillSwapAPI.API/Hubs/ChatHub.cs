using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SkillSwapAPI.Application.Features.Chat.Commands.SendMessage;
using SkillSwapAPI.Application.Features.Chat.Dtos;
using SkillSwapAPI.Application.Features.Chat.Queries.GetConversationAccess;

namespace SkillSwapAPI.API.Hubs;

[Authorize]
public sealed class ChatHub(ISender mediator) : Hub
{
    public async Task JoinConversation(Guid conversationId)
    {
        var userId = GetUserId();

        var result = await mediator.Send(new GetConversationAccessQuery(conversationId, userId), Context.ConnectionAborted);

        if (result.IsError)
        {
            throw new HubException(result.TopError.Description);
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, ConversationGroupName(conversationId), Context.ConnectionAborted);
    }

    public async Task LeaveConversation(Guid conversationId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, ConversationGroupName(conversationId), Context.ConnectionAborted);
    }

    public async Task<MessageDto> SendMessage(Guid conversationId, string content)
    {
        var userId = GetUserId();

        var result = await mediator.Send(new SendMessageCommand(conversationId, userId, content), Context.ConnectionAborted);

        if (result.IsError)
        {
            throw new HubException(result.TopError.Description);
        }

        await Clients.Group(ConversationGroupName(conversationId))
            .SendAsync("ReceiveMessage", result.Value, Context.ConnectionAborted);

        return result.Value;
    }

    public static string ConversationGroupName(Guid conversationId)
    {
        return $"conversation-{conversationId}";
    }

    private Guid GetUserId()
    {
        var value = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? Context.User?.FindFirstValue("sub");

        if (!Guid.TryParse(value, out var userId))
        {
            throw new HubException("Token does not contain a valid user identifier.");
        }

        return userId;
    }
}
