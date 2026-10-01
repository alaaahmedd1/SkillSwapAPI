using MediatR;
using SkillSwapAPI.Application.Common.Enums;
using SkillSwapAPI.Application.Common.Errors;
using SkillSwapAPI.Application.Common.Interfaces.Identity;
using SkillSwapAPI.Application.Common.Interfaces.UnitOfWork;
using SkillSwapAPI.Application.Features.SessionProposals.Dtos;
using SkillSwapAPI.Application.Features.SwapRequests.Dtos;
using SkillSwapAPI.Domain.Common.Results;

namespace SkillSwapAPI.Application.Features.SwapRequests.Queries.GetSwapRequestDetails;

public sealed class GetSwapRequestDetailsQueryHandler(
    IUnitOfWork unitOfWork,
    IIdentityService identityService)
    : IRequestHandler<GetSwapRequestDetailsQuery, Result<SwapRequestDetailsDto>>
{
    public async Task<Result<SwapRequestDetailsDto>> Handle(GetSwapRequestDetailsQuery query, CancellationToken ct)
    {
        var swapRequest = await unitOfWork.SwapRequests.GetByIdWithDetailsAsync(query.SwapRequestId, ct);

        if (swapRequest is null)
        {
            return ApplicationErrors.SwapRequests.NotFound;
        }

        if (swapRequest.RequesterId != query.UserId && swapRequest.ReceiverId != query.UserId)
        {
            return ApplicationErrors.SwapRequests.NotParticipant;
        }

        var profiles = await identityService.GetProfilesAsync(
            [swapRequest.RequesterId, swapRequest.ReceiverId], ct);

        var requester = profiles.FirstOrDefault(profile => profile.UserId == swapRequest.RequesterId);
        var receiver = profiles.FirstOrDefault(profile => profile.UserId == swapRequest.ReceiverId);

        var conversation = await unitOfWork.Conversations.FindAsync(
            candidate => candidate.SwapRequestId == swapRequest.Id, ct);

        var sessionProposals = await unitOfWork.SessionProposals.FindAllAsync(
            proposal => proposal.SwapRequestId == swapRequest.Id,
            SortDirection.Descending,
            proposal => proposal.CreatedAtUtc,
            ct);

        return new SwapRequestDetailsDto(
            swapRequest.Id,
            swapRequest.RequesterId,
            swapRequest.ReceiverId,
            requester?.FirstName ?? string.Empty,
            requester?.LastName ?? string.Empty,
            receiver?.FirstName ?? string.Empty,
            receiver?.LastName ?? string.Empty,
            new SwapRequestSkillDto(
                swapRequest.OfferedSkill.Id,
                swapRequest.OfferedSkill.Name,
                swapRequest.OfferedSkill.Category.Name),
            new SwapRequestSkillDto(
                swapRequest.RequestedSkill.Id,
                swapRequest.RequestedSkill.Name,
                swapRequest.RequestedSkill.Category.Name),
            swapRequest.Status,
            swapRequest.IsRequesterConfirmed,
            swapRequest.IsReceiverConfirmed,
            swapRequest.ProposedScheduleDetails,
            swapRequest.CreatedAtUtc,
            swapRequest.UpdatedAtUtc,
            conversation?.Id,
            sessionProposals
                .Select(proposal => new SessionProposalDto(
                    proposal.Id,
                    proposal.SwapRequestId,
                    proposal.ProposerId,
                    proposal.ScheduledDate,
                    proposal.StartTime,
                    proposal.EndTime,
                    proposal.DurationMinutes,
                    proposal.Status,
                    proposal.CreatedAtUtc))
                .ToList());
    }
}
