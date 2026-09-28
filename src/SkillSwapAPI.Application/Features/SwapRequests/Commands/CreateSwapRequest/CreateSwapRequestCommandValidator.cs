using FluentValidation;
using SkillSwapAPI.Application.Common.Interfaces.UnitOfWork;
using SkillSwapAPI.Domain.Modules.Users.Enums;

namespace SkillSwapAPI.Application.Features.SwapRequests.Commands.CreateSwapRequest;

public sealed class CreateSwapRequestCommandValidator : AbstractValidator<CreateSwapRequestCommand>
{
    public CreateSwapRequestCommandValidator(IUnitOfWork unitOfWork)
    {
        RuleFor(command => command.RequesterId).NotEmpty();
        RuleFor(command => command.ReceiverId)
            .NotEmpty()
            .NotEqual(command => command.RequesterId)
            .WithMessage("A user cannot send a swap request to themselves.");
        RuleFor(command => command.OfferedSkillId).NotEmpty();
        RuleFor(command => command.RequestedSkillId).NotEmpty();
        RuleFor(command => command.ProposedScheduleDetails).MaximumLength(1000);

        RuleFor(command => command.OfferedSkillId)
            .MustAsync(async (skillId, ct) => await unitOfWork.Skills.ExistsAsync(skillId, ct))
            .WithMessage("The offered skill does not exist.")
            .When(command => command.OfferedSkillId != Guid.Empty);

        RuleFor(command => command.RequestedSkillId)
            .MustAsync(async (command, requestedSkillId, ct) => await unitOfWork.UserSkills.HasSkillAsync(
                command.ReceiverId, requestedSkillId, SkillType.Offered, ct))
            .WithMessage("The receiver does not offer the requested skill.")
            .When(command => command.RequestedSkillId != Guid.Empty && command.ReceiverId != Guid.Empty);

        RuleFor(command => command)
            .MustAsync(async (command, ct) => !await unitOfWork.SwapRequests.HasPendingDuplicateAsync(
                command.RequesterId, command.ReceiverId, command.OfferedSkillId, command.RequestedSkillId, ct))
            .WithMessage("A pending swap request to the same receiver for the same skills already exists.")
            .OverridePropertyName("SwapRequest")
            .When(command => command.RequesterId != Guid.Empty && command.ReceiverId != Guid.Empty);
    }
}
