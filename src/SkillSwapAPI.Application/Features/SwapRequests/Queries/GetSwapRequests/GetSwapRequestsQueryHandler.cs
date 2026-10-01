using MediatR;
using SkillSwapAPI.Application.Common.Interfaces.Identity;
using SkillSwapAPI.Application.Common.Interfaces.UnitOfWork;
using SkillSwapAPI.Application.Common.Models;
using SkillSwapAPI.Application.Features.SwapRequests.Dtos;
using SkillSwapAPI.Domain.Common.Results;

namespace SkillSwapAPI.Application.Features.SwapRequests.Queries.GetSwapRequests;

public sealed class GetSwapRequestsQueryHandler(
    IUnitOfWork unitOfWork,
    IIdentityService identityService)
    : IRequestHandler<GetSwapRequestsQuery, Result<PagedResult<SwapRequestDto>>>
{
    public async Task<Result<PagedResult<SwapRequestDto>>> Handle(GetSwapRequestsQuery query, CancellationToken ct)
    {
        var (pageItems, totalCount) = await unitOfWork.SwapRequests.GetPagedForUserAsync(
            query.UserId, query.Status, query.PageNumber, query.PageSize, ct);

        var participantIds = pageItems
            .SelectMany(swapRequest => new[] { swapRequest.RequesterId, swapRequest.ReceiverId })
            .Distinct()
            .ToList();

        var profiles = await identityService.GetProfilesAsync(participantIds, ct);

        var swapIds = pageItems.Select(swapRequest => swapRequest.Id).ToList();
        var conversations = await unitOfWork.Conversations.FindAllAsync(
            conversation => swapIds.Contains(conversation.SwapRequestId), ct);
        var conversationIdsBySwapId = conversations.ToDictionary(
            conversation => conversation.SwapRequestId, conversation => (Guid?)conversation.Id);

        var items = pageItems
            .Select(swapRequest =>
            {
                var requester = profiles.FirstOrDefault(profile => profile.UserId == swapRequest.RequesterId);
                var receiver = profiles.FirstOrDefault(profile => profile.UserId == swapRequest.ReceiverId);

                return new SwapRequestDto(
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
                    swapRequest.CreatedAtUtc,
                    swapRequest.UpdatedAtUtc,
                    conversationIdsBySwapId.GetValueOrDefault(swapRequest.Id));
            })
            .ToList();

        return PagedResult<SwapRequestDto>.Create(items, totalCount, query.PageNumber, query.PageSize);
    }
}
