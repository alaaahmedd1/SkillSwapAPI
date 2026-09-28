using MediatR;
using SkillSwapAPI.Application.Common.Interfaces.Identity;
using SkillSwapAPI.Application.Common.Interfaces.Services;
using SkillSwapAPI.Application.Common.Interfaces.UnitOfWork;
using SkillSwapAPI.Application.Features.Users.Dtos;
using SkillSwapAPI.Domain.Common.Results;
using SkillSwapAPI.Domain.Modules.Administration.Entities;
using SkillSwapAPI.Domain.Modules.SwapRequests.Enums;

namespace SkillSwapAPI.Application.Features.Admin.Commands.UpdateUserStatus;

public sealed class UpdateUserStatusCommandHandler(
    IUnitOfWork unitOfWork,
    IIdentityService identityService,
    IChatConnectionManager chatConnectionManager)
    : IRequestHandler<UpdateUserStatusCommand, Result<ProfileIdentityDto>>
{
    public async Task<Result<ProfileIdentityDto>> Handle(UpdateUserStatusCommand command, CancellationToken ct)
    {
        var updatedUser = await identityService.UpdateUserStatusAsync(command.UserId, command.IsActive, ct);

        if (updatedUser.IsError)
        {
            return updatedUser.TopError;
        }

        if (command.IsActive)
        {
            await unitOfWork.AuditLogs.AddAsync(new AuditLog
            {
                Id = Guid.NewGuid(),
                AdminId = command.AdminId,
                Action = "UnbanUser",
                TargetEntity = "ApplicationUser",
                TargetEntityId = command.UserId.ToString(),
                PerformedAtUtc = DateTimeOffset.UtcNow
            }, ct);

            await unitOfWork.CompleteAsync(ct);

            return updatedUser.Value;
        }

        var activeTokens = (await unitOfWork.RefreshTokens.GetActiveByUserAsync(
            command.UserId.ToString(), ct)).ToList();

        foreach (var activeToken in activeTokens)
        {
            activeToken.IsRevoked = true;
        }

        unitOfWork.RefreshTokens.UpdateRange(activeTokens);

        var activeSwapRequests = await unitOfWork.SwapRequests.GetActiveByUserAsync(command.UserId, ct);

        foreach (var swapRequest in activeSwapRequests)
        {
            swapRequest.Status = SwapRequestStatus.Cancelled;
            swapRequest.UpdatedAtUtc = DateTimeOffset.UtcNow;
        }

        unitOfWork.SwapRequests.UpdateRange(activeSwapRequests);

        await unitOfWork.AuditLogs.AddAsync(new AuditLog
        {
            Id = Guid.NewGuid(),
            AdminId = command.AdminId,
            Action = "BanUser",
            TargetEntity = "ApplicationUser",
            TargetEntityId = command.UserId.ToString(),
            Reason = "System: User Account Suspended",
            PerformedAtUtc = DateTimeOffset.UtcNow
        }, ct);

        await unitOfWork.CompleteAsync(ct);

        await chatConnectionManager.DisconnectUserAsync(command.UserId, ct);

        return updatedUser.Value;
    }
}
