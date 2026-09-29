using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSwapAPI.Application.Features.SessionProposals.Commands.AcceptProposal;
using SkillSwapAPI.Application.Features.SessionProposals.Commands.CreateProposal;
using SkillSwapAPI.Application.Features.SessionProposals.Commands.RejectProposal;
using SkillSwapAPI.Application.Features.SwapRequests.Commands.AcceptSwapRequest;
using SkillSwapAPI.Application.Features.SwapRequests.Commands.CancelSwapRequest;
using SkillSwapAPI.Application.Features.SwapRequests.Commands.CompleteSwapRequest;
using SkillSwapAPI.Application.Features.SwapRequests.Commands.CreateSwapRequest;
using SkillSwapAPI.Application.Features.SwapRequests.Commands.RejectSwapRequest;
using SkillSwapAPI.Application.Features.SwapRequests.Queries.GetSwapRequestDetails;
using SkillSwapAPI.Application.Features.SwapRequests.Queries.GetSwapRequests;
using SkillSwapAPI.Domain.Modules.SwapRequests.Enums;

namespace SkillSwapAPI.API.Controllers.SwapRequests;

[Authorize]
[Route("api/v1/swap-requests")]
public sealed class SwapRequestsController : ApiBaseController
{
    [HttpPost]
    public async Task<IActionResult> CreateSwapRequest([FromBody] CreateSwapRequestRequest request, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        var result = await Mediator.Send(
            new CreateSwapRequestCommand(
                userId,
                request.ReceiverId,
                request.OfferedSkillId,
                request.RequestedSkillId,
                request.ProposedScheduleDetails), ct);

        if (result.IsError)
        {
            return HandleResult(result);
        }

        return StatusCode(StatusCodes.Status201Created, result.Value);
    }

    [HttpGet]
    public async Task<IActionResult> GetSwapRequests([FromQuery] GetSwapRequestsRequest request, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        return HandleResult(await Mediator.Send(
            new GetSwapRequestsQuery(userId, request.Status, request.PageNumber, request.PageSize), ct));
    }

    [HttpGet("{swapRequestId:guid}")]
    public async Task<IActionResult> GetSwapRequestDetails(Guid swapRequestId, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        return HandleResult(await Mediator.Send(
            new GetSwapRequestDetailsQuery(swapRequestId, userId), ct));
    }

    [HttpPut("{swapRequestId:guid}/accept")]
    public async Task<IActionResult> AcceptSwapRequest(Guid swapRequestId, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        return HandleResult(await Mediator.Send(new AcceptSwapRequestCommand(swapRequestId, userId), ct));
    }

    [HttpPut("{swapRequestId:guid}/reject")]
    public async Task<IActionResult> RejectSwapRequest(Guid swapRequestId, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        return HandleResult(await Mediator.Send(new RejectSwapRequestCommand(swapRequestId, userId), ct));
    }

    [HttpPut("{swapRequestId:guid}/cancel")]
    public async Task<IActionResult> CancelSwapRequest(Guid swapRequestId, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        return HandleResult(await Mediator.Send(new CancelSwapRequestCommand(swapRequestId, userId), ct));
    }

    [HttpPut("{swapRequestId:guid}/complete")]
    public async Task<IActionResult> CompleteSwapRequest(Guid swapRequestId, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        return HandleResult(await Mediator.Send(new CompleteSwapRequestCommand(swapRequestId, userId), ct));
    }

    [HttpPost("{id:guid}/proposals")]
    public async Task<IActionResult> CreateProposal(Guid id, [FromBody] CreateProposalRequest request, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        var result = await Mediator.Send(
            new CreateProposalCommand(
                id,
                userId,
                request.ScheduledDate,
                request.StartTime,
                request.EndTime,
                request.DurationMinutes), ct);

        if (result.IsError)
        {
            return HandleResult(result);
        }

        return StatusCode(StatusCodes.Status201Created, result.Value);
    }

    [HttpPut("{id:guid}/proposals/{proposalId:guid}/accept")]
    public async Task<IActionResult> AcceptProposal(Guid id, Guid proposalId, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        return HandleResult(await Mediator.Send(new AcceptProposalCommand(id, proposalId, userId), ct));
    }

    [HttpPut("{id:guid}/proposals/{proposalId:guid}/reject")]
    public async Task<IActionResult> RejectProposal(Guid id, Guid proposalId, CancellationToken ct)
    {
        if (!TryGetCurrentUserId(out var userId))
        {
            return Unauthorized();
        }

        return HandleResult(await Mediator.Send(new RejectProposalCommand(id, proposalId, userId), ct));
    }
}

public sealed record CreateSwapRequestRequest(
    Guid ReceiverId,
    Guid OfferedSkillId,
    Guid RequestedSkillId,
    string? ProposedScheduleDetails);

public sealed record GetSwapRequestsRequest(
    SwapRequestStatus? Status = null,
    int PageNumber = 1,
    int PageSize = 10);

public sealed record CreateProposalRequest(
    DateOnly ScheduledDate,
    TimeOnly StartTime,
    TimeOnly EndTime,
    int DurationMinutes);
