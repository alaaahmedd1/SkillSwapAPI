using MediatR;
using SkillSwapAPI.Application.Common.Interfaces.UnitOfWork;
using SkillSwapAPI.Application.Features.Badges.Dtos;
using SkillSwapAPI.Domain.Common.Results;

namespace SkillSwapAPI.Application.Features.Badges.Queries.GetBadges;

public sealed class GetBadgesQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetBadgesQuery, Result<IReadOnlyList<BadgeDto>>>
{
    public async Task<Result<IReadOnlyList<BadgeDto>>> Handle(GetBadgesQuery query, CancellationToken ct)
    {
        var badges = await unitOfWork.Badges.GetActiveAsync(ct);

        var items = badges
            .Select(badge => new BadgeDto(
                badge.Id,
                badge.Name,
                badge.Description,
                badge.IconUrl))
            .ToList();

        return items;
    }
}
