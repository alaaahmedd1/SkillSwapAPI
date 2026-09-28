using MediatR;
using SkillSwapAPI.Application.Common.Interfaces.Identity;
using SkillSwapAPI.Application.Common.Interfaces.UnitOfWork;
using SkillSwapAPI.Application.Common.Models;
using SkillSwapAPI.Application.Features.Users.Dtos;
using SkillSwapAPI.Domain.Common.Results;

namespace SkillSwapAPI.Application.Features.Users.Queries.SearchUsers;

public sealed class SearchUsersQueryHandler(
    IUnitOfWork unitOfWork,
    IIdentityService identityService)
    : IRequestHandler<SearchUsersQuery, Result<PagedResult<UserSearchResultDto>>>
{
    public async Task<Result<PagedResult<UserSearchResultDto>>> Handle(SearchUsersQuery query, CancellationToken ct)
    {
        var candidateIds = await unitOfWork.UserSkills.GetUserIdsByFiltersAsync(
            query.CategoryId, query.OfferedSkillId, query.SeekingSkillId, query.ProficiencyLevel, ct);

        var profiles = await identityService.GetProfilesAsync(candidateIds, ct);
        var filteredProfiles = profiles
            .Where(profile => profile.IsActive
                && (!query.MinRating.HasValue || profile.AverageRating >= query.MinRating.Value)
                && (string.IsNullOrWhiteSpace(query.SearchTerm)
                    || profile.FirstName.Contains(query.SearchTerm, StringComparison.OrdinalIgnoreCase)
                    || profile.LastName.Contains(query.SearchTerm, StringComparison.OrdinalIgnoreCase)))
            .OrderByDescending(profile => profile.AverageRating)
            .ThenBy(profile => profile.FirstName)
            .ToList();

        var totalCount = filteredProfiles.Count;
        var pageProfiles = filteredProfiles
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

        var pageUserIds = pageProfiles
            .Select(profile => profile.UserId)
            .ToList();

        var pageUserSkills = await unitOfWork.UserSkills.GetByUserIdsAsync(pageUserIds, ct);

        var skillsLookup = pageUserSkills
            .GroupBy(userSkill => userSkill.UserId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<UserSkillSummaryDto>)group
                    .Select(userSkill => new UserSkillSummaryDto(
                        userSkill.SkillId,
                        userSkill.Skill.Name,
                        userSkill.Skill.Category.Name,
                        userSkill.Type,
                        userSkill.ProficiencyLevel))
                    .ToList());

        var items = pageProfiles
            .Select(profile => new UserSearchResultDto(
                profile.UserId,
                profile.FirstName,
                profile.LastName,
                profile.AverageRating,
                profile.TotalReviewsCount,
                skillsLookup.GetValueOrDefault(profile.UserId, [])))
            .ToList();

        return PagedResult<UserSearchResultDto>.Create(items, totalCount, query.PageNumber, query.PageSize);
    }
}
