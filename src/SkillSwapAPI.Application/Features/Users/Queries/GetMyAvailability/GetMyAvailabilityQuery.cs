using MediatR;
using SkillSwapAPI.Application.Common.Interfaces.UnitOfWork;
using SkillSwapAPI.Application.Features.Users.Dtos;
using SkillSwapAPI.Domain.Common.Results;
using SkillSwapAPI.Domain.Modules.Users.Entities;

namespace SkillSwapAPI.Application.Features.Users.Queries.GetMyAvailability;

public sealed record GetMyAvailabilityQuery(Guid UserId)
    : IRequest<Result<IReadOnlyList<AvailabilitySlotDto>>>;

public sealed class GetMyAvailabilityQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetMyAvailabilityQuery, Result<IReadOnlyList<AvailabilitySlotDto>>>
{
    public async Task<Result<IReadOnlyList<AvailabilitySlotDto>>> Handle(
        GetMyAvailabilityQuery query,
        CancellationToken ct)
    {
        var availability = await unitOfWork.UserAvailabilities.GetByUserAsync(query.UserId, ct);

        return availability
            .Select(row => new AvailabilitySlotDto(row.DayOfWeek, row.TimeBlock))
            .ToList();
    }
}
