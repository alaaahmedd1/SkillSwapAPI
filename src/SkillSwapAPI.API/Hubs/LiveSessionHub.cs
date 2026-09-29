using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SkillSwapAPI.Application.Features.LiveSessions.Commands.SaveWhiteboardSnapshot;
using SkillSwapAPI.Application.Features.LiveSessions.Commands.StartLiveSession;
using SkillSwapAPI.Application.Features.LiveSessions.Dtos;
using SkillSwapAPI.Application.Features.LiveSessions.Queries.GetLiveSessionAccess;
using SkillSwapAPI.Application.Features.LiveSessions.Queries.GetLiveSessionElapsed;
using System.Security.Claims;

namespace SkillSwapAPI.API.Hubs;

[Authorize]
public sealed class LiveSessionHub(ISender mediator) : Hub
{
    public async Task JoinRoom(Guid swapId)
    {
        await VerifyAccessAsync(swapId);

        await Groups.AddToGroupAsync(Context.ConnectionId, RoomGroupName(swapId), Context.ConnectionAborted);
    }

    public async Task LeaveRoom(Guid swapId)
    {
        await VerifyAccessAsync(swapId);

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, RoomGroupName(swapId), Context.ConnectionAborted);
    }

    public async Task<LiveSessionRoomDto> StartSession(Guid swapId)
    {
        var result = await mediator.Send(
            new StartLiveSessionCommand(swapId, GetUserId()), Context.ConnectionAborted);

        if (result.IsError)
        {
            throw new HubException(result.TopError.Description);
        }

        await Clients.Group(RoomGroupName(swapId))
            .SendAsync("SessionStarted", result.Value, Context.ConnectionAborted);

        return result.Value;
    }

    public async Task<LiveSessionTimerDto> SendSessionHeartbeat(Guid swapId)
    {
        var result = await mediator.Send(
            new GetLiveSessionElapsedQuery(swapId, GetUserId()), Context.ConnectionAborted);

        if (result.IsError)
        {
            throw new HubException(result.TopError.Description);
        }

        return result.Value;
    }

    public async Task SendOffer(Guid swapId, RtcSessionDescriptionDto offer)
    {
        if (string.IsNullOrWhiteSpace(offer.Sdp))
        {
            throw new HubException("Offer SDP payload cannot be empty.");
        }

        await VerifyAccessAsync(swapId);

        await Clients.OthersInGroup(RoomGroupName(swapId))
            .SendAsync("ReceiveOffer", offer, Context.ConnectionAborted);
    }

    public async Task SendAnswer(Guid swapId, RtcSessionDescriptionDto answer)
    {
        if (string.IsNullOrWhiteSpace(answer.Sdp))
        {
            throw new HubException("Answer SDP payload cannot be empty.");
        }

        await VerifyAccessAsync(swapId);

        await Clients.OthersInGroup(RoomGroupName(swapId))
            .SendAsync("ReceiveAnswer", answer, Context.ConnectionAborted);
    }

    public async Task SendIceCandidate(Guid swapId, RtcIceCandidateDto candidate)
    {
        if (string.IsNullOrWhiteSpace(candidate.Candidate))
        {
            throw new HubException("ICE candidate payload cannot be empty.");
        }

        await VerifyAccessAsync(swapId);

        await Clients.OthersInGroup(RoomGroupName(swapId))
            .SendAsync("ReceiveIceCandidate", candidate, Context.ConnectionAborted);
    }

    public async Task SendWhiteboardOperation(Guid swapId, string operation)
    {
        if (string.IsNullOrWhiteSpace(operation))
        {
            throw new HubException("Whiteboard operation payload cannot be empty.");
        }

        await VerifyAccessAsync(swapId);

        await Clients.OthersInGroup(RoomGroupName(swapId))
            .SendAsync("ReceiveWhiteboardOperation", operation, Context.ConnectionAborted);
    }

    public async Task<WhiteboardSnapshotDto> SaveWhiteboardSnapshot(Guid swapId, string canvasDataJson)
    {
        var result = await mediator.Send(
            new SaveWhiteboardSnapshotCommand(swapId, GetUserId(), canvasDataJson), Context.ConnectionAborted);

        if (result.IsError)
        {
            throw new HubException(result.TopError.Description);
        }

        return result.Value;
    }

    public static string RoomGroupName(Guid swapId)
    {
        return $"live-session-{swapId}";
    }

    private async Task VerifyAccessAsync(Guid swapId)
    {
        var result = await mediator.Send(
            new GetLiveSessionAccessQuery(swapId, GetUserId()), Context.ConnectionAborted);

        if (result.IsError)
        {
            throw new HubException(result.TopError.Description);
        }
    }

    private Guid GetUserId()
    {
        if (!TryGetUserId(out var userId))
        {
            throw new HubException("Token does not contain a valid user identifier.");
        }

        return userId;
    }

    private bool TryGetUserId(out Guid userId)
    {
        var value = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? Context.User?.FindFirstValue("sub");

        return Guid.TryParse(value, out userId);
    }
}
