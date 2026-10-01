using SkillSwapAPI.Domain.Modules.Users.Enums;

namespace SkillSwapAPI.Application.Features.Users.Dtos;

public sealed record AvailabilitySlotDto(
    DayOfWeek DayOfWeek,
    TimeBlock TimeBlock);
