using MediatR;
using SkillSwapAPI.Application.Common.Interfaces.UnitOfWork;
using SkillSwapAPI.Application.Features.Badges.Dtos;
using SkillSwapAPI.Domain.Common.Results;

namespace SkillSwapAPI.Application.Features.Badges.Queries.GetUserBadges;

public sealed class GetUserBadgesQueryHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetUserBadgesQuery, Result<IReadOnlyList<UserBadgeDto>>>
{
    public async Task<Result<IReadOnlyList<UserBadgeDto>>> Handle(GetUserBadgesQuery query, CancellationToken ct)
    {
        var awards = await unitOfWork.UserBadgeAwards.GetByRevieweeAsync(query.UserId, ct);

        List<UserBadgeDto> items = [];

        if (awards.Count > 0)
        {
            var badgeIds = awards
                .Select(award => award.BadgeId)
                .Distinct()
                .ToList();

            var badges = await unitOfWork.Badges.GetByIdsAsync(badgeIds, ct);

            var awardSummaries = awards
                .GroupBy(award => award.BadgeId)
                .Select(group => new
                {
                    BadgeId = group.Key,
                    AwardCount = group.Count(),
                    LastAwardedAtUtc = group.Max(award => award.AwardedAtUtc)
                });

            items = awardSummaries
                .Join(
                    badges,
                    summary => summary.BadgeId,
                    badge => badge.Id,
                    (summary, badge) => new UserBadgeDto(
                        badge.Id,
                        badge.Name,
                        badge.Description,
                        badge.IconUrl,
                        summary.AwardCount,
                        summary.LastAwardedAtUtc))
                .OrderByDescending(dto => dto.AwardCount)
                .ThenBy(dto => dto.BadgeId)
                .ToList();
        }

        return items;
    }
}
