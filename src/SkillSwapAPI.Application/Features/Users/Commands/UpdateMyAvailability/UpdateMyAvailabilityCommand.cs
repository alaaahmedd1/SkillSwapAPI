using MediatR;
using FluentValidation;
using SkillSwapAPI.Application.Common.Interfaces.UnitOfWork;
using SkillSwapAPI.Application.Features.Users.Dtos;
using SkillSwapAPI.Domain.Common.Results;
using SkillSwapAPI.Domain.Modules.Users.Entities;
using SkillSwapAPI.Domain.Modules.Users.Enums;

namespace SkillSwapAPI.Application.Features.Users.Commands.UpdateMyAvailability;

public sealed record UpdateMyAvailabilityCommand(
    Guid UserId,
    IReadOnlyList<AvailabilitySlotDto> Slots)
    : IRequest<Result<Updated>>;

public sealed class UpdateMyAvailabilityCommandValidator : AbstractValidator<UpdateMyAvailabilityCommand>
{
    private const int MaxSlots = 21; // 7 days x 3 time blocks

    public UpdateMyAvailabilityCommandValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();
        RuleFor(command => command.Slots).NotNull().Must(slots => slots.Count <= MaxSlots)
            .WithMessage($"At most {MaxSlots} availability slots are allowed.");
        RuleFor(command => command.Slots)
            .Must(slots => slots.Select(slot => (slot.DayOfWeek, slot.TimeBlock))
                .Distinct().Count() == slots.Count)
            .WithMessage("Duplicate availability slots are not allowed.");
        RuleForEach(command => command.Slots).ChildRules(slot =>
        {
            slot.RuleFor(s => s.TimeBlock).IsInEnum();
        });
    }
}

public sealed class UpdateMyAvailabilityCommandHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateMyAvailabilityCommand, Result<Updated>>
{
    public async Task<Result<Updated>> Handle(UpdateMyAvailabilityCommand command, CancellationToken ct)
    {
        var availability = command.Slots
            .Select(slot => new UserAvailability
            {
                Id = Guid.NewGuid(),
                UserId = command.UserId,
                DayOfWeek = slot.DayOfWeek,
                TimeBlock = slot.TimeBlock
            })
            .ToList();

        await unitOfWork.UserAvailabilities.ReplaceForUserAsync(command.UserId, availability, ct);
        await unitOfWork.CompleteAsync(ct);

        return Result.Updated;
    }
}
