using MediatR;
using SkillSwapAPI.Application.Common.Interfaces.Identity;
using SkillSwapAPI.Application.Common.Interfaces.UnitOfWork;
using SkillSwapAPI.Application.Features.SwapRequests.Dtos;
using SkillSwapAPI.Domain.Common.Results;
using SkillSwapAPI.Domain.Modules.SwapRequests.Entities;
using SkillSwapAPI.Domain.Modules.SwapRequests.Enums;

namespace SkillSwapAPI.Application.Features.SwapRequests.Commands.CreateSwapRequest;

public sealed class CreateSwapRequestCommandHandler(
    IUnitOfWork unitOfWork,
    IIdentityService identityService)
    : IRequestHandler<CreateSwapRequestCommand, Result<SwapRequestDto>>
{
    public async Task<Result<SwapRequestDto>> Handle(CreateSwapRequestCommand command, CancellationToken ct)
    {
        var swapRequest = new SwapRequest
        {
            Id = Guid.NewGuid(),
            RequesterId = command.RequesterId,
            ReceiverId = command.ReceiverId,
            OfferedSkillId = command.OfferedSkillId,
            RequestedSkillId = command.RequestedSkillId,
            Status = SwapRequestStatus.Pending,
            IsRequesterConfirmed = false,
            IsReceiverConfirmed = false,
            ProposedScheduleDetails = command.ProposedScheduleDetails,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        await unitOfWork.SwapRequests.AddAsync(swapRequest, ct);
        await unitOfWork.CompleteAsync(ct);

        var skills = await unitOfWork.Skills.GetByIdsWithCategoryAsync(
            [command.OfferedSkillId, command.RequestedSkillId], ct);

        var profiles = await identityService.GetProfilesAsync(
            [command.RequesterId, command.ReceiverId], ct);

        var requester = profiles.FirstOrDefault(profile => profile.UserId == command.RequesterId);
        var receiver = profiles.FirstOrDefault(profile => profile.UserId == command.ReceiverId);
        var offeredSkill = skills.First(skill => skill.Id == command.OfferedSkillId);
        var requestedSkill = skills.First(skill => skill.Id == command.RequestedSkillId);

        return new SwapRequestDto(
            swapRequest.Id,
            swapRequest.RequesterId,
            swapRequest.ReceiverId,
            requester?.FirstName ?? string.Empty,
            requester?.LastName ?? string.Empty,
            receiver?.FirstName ?? string.Empty,
            receiver?.LastName ?? string.Empty,
            new SwapRequestSkillDto(offeredSkill.Id, offeredSkill.Name, offeredSkill.Category.Name),
            new SwapRequestSkillDto(requestedSkill.Id, requestedSkill.Name, requestedSkill.Category.Name),
            swapRequest.Status,
            swapRequest.IsRequesterConfirmed,
            swapRequest.IsReceiverConfirmed,
            swapRequest.CreatedAtUtc,
            swapRequest.UpdatedAtUtc,
            ConversationId: null);
    }
}
