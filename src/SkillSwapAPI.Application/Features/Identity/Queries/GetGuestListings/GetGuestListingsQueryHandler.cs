using MediatR;
using SkillSwapAPI.Application.Common.Interfaces.Identity;
using SkillSwapAPI.Application.Common.Interfaces.UnitOfWork;
using SkillSwapAPI.Application.Common.Models;
using SkillSwapAPI.Domain.Common.Results;
using SkillSwapAPI.Domain.Modules.Users.Enums;

namespace SkillSwapAPI.Application.Features.Identity.Queries.GetGuestListings;

public sealed class GetGuestListingsQueryHandler(IUnitOfWork unitOfWork, IIdentityService identityService)
    : IRequestHandler<GetGuestListingsQuery, Result<PagedResult<PublicListingDto>>>
{
    public async Task<Result<PagedResult<PublicListingDto>>> Handle(
        GetGuestListingsQuery query,
        CancellationToken ct)
    {
        // Safe, sanitized guest listings query (read-only, no PII, no email/wallet data exposed)
        var userIds = await unitOfWork.UserSkills.GetUserIdsByFiltersAsync(
            categoryId: null,
            offeredSkillId: null,
            seekingSkillId: null,
            proficiencyLevel: null,
            ct);

        if (userIds.Count == 0)
        {
            return PagedResult<PublicListingDto>.Create(
                [], 0, query.PageNumber, query.PageSize);
        }

        var profiles = await identityService.GetProfilesAsync(userIds, ct);
        var userSkills = await unitOfWork.UserSkills.GetByUserIdsAsync(userIds, ct);

        var offeredByUser = userSkills
            .Where(userSkill => userSkill.Type == SkillType.Offered)
            .GroupBy(userSkill => userSkill.UserId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<string>)group
                    .Select(userSkill => userSkill.Skill.Name)
                    .Distinct()
                    .ToList());

        var wantedByUser = userSkills
            .Where(userSkill => userSkill.Type == SkillType.Seeking)
            .GroupBy(userSkill => userSkill.UserId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<string>)group
                    .Select(userSkill => userSkill.Skill.Name)
                    .Distinct()
                    .ToList());

        var items = profiles
            .Select(profile => new PublicListingDto(
                profile.UserId.ToString(),
                $"{profile.FirstName} {profile.LastName}".Trim(),
                profile.Title ?? string.Empty,
                profile.Country ?? string.Empty,
                offeredByUser.GetValueOrDefault(profile.UserId, []),
                wantedByUser.GetValueOrDefault(profile.UserId, []),
                profile.City ?? string.Empty,
                profile.AverageRating,
                profile.TotalReviewsCount))
            .ToList();

        if (!string.IsNullOrWhiteSpace(query.SkillFilter))
        {
            items = items
                .Where(x => x.SkillsOffered.Concat(x.SkillsWanted)
                    .Any(s => s.Contains(query.SkillFilter, StringComparison.OrdinalIgnoreCase)))
                .ToList();
        }

        items = items
            .OrderByDescending(x => x.AverageRating)
            .ThenBy(x => x.DisplayName)
            .ToList();

        var totalCount = items.Count;
        var pageItems = items
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

        return PagedResult<PublicListingDto>.Create(
            pageItems,
            totalCount,
            query.PageNumber,
            query.PageSize);
    }
}
