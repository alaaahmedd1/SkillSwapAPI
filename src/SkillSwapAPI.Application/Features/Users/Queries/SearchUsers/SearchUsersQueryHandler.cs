using MediatR;
using Microsoft.EntityFrameworkCore;
using SkillSwapAPI.Application.Common.Interfaces;
using SkillSwapAPI.Application.Common.Interfaces.Identity;
using SkillSwapAPI.Application.Common.Models;
using SkillSwapAPI.Application.Features.Users.Dtos;
using SkillSwapAPI.Domain.Common.Results;
using SkillSwapAPI.Domain.Modules.Users.Enums;

namespace SkillSwapAPI.Application.Features.Users.Queries.SearchUsers;

public sealed class SearchUsersQueryHandler(
    IApplicationDbContext context,
    IIdentityService identityService)
    : IRequestHandler<SearchUsersQuery, Result<PagedResult<UserSearchResultDto>>>
{
    public async Task<Result<PagedResult<UserSearchResultDto>>> Handle(SearchUsersQuery query, CancellationToken ct)
    {
        var userSkillsQuery = context.UserSkills
            .AsNoTracking()
            .Include(userSkill => userSkill.Skill)
                .ThenInclude(skill => skill.Category)
            .AsQueryable();

        var offeredUserIds = userSkillsQuery
            .Where(userSkill => userSkill.Type == SkillType.Offered)
            .Select(userSkill => userSkill.UserId)
            .Distinct();

        if (query.CategoryId.HasValue)
        {
            offeredUserIds = offeredUserIds.Where(userId => userSkillsQuery.Any(
                userSkill => userSkill.UserId == userId && userSkill.Skill.CategoryId == query.CategoryId.Value));
        }

        if (query.OfferedSkillId.HasValue)
        {
            offeredUserIds = offeredUserIds.Where(userId => userSkillsQuery.Any(
                userSkill => userSkill.UserId == userId
                    && userSkill.Type == SkillType.Offered
                    && userSkill.SkillId == query.OfferedSkillId.Value));
        }

        if (query.SeekingSkillId.HasValue)
        {
            offeredUserIds = offeredUserIds.Where(userId => userSkillsQuery.Any(
                userSkill => userSkill.UserId == userId
                    && userSkill.Type == SkillType.Seeking
                    && userSkill.SkillId == query.SeekingSkillId.Value));
        }

        if (query.ProficiencyLevel.HasValue)
        {
            offeredUserIds = offeredUserIds.Where(userId => userSkillsQuery.Any(
                userSkill => userSkill.UserId == userId
                    && userSkill.ProficiencyLevel == query.ProficiencyLevel.Value));
        }

        var candidateIds = await offeredUserIds.ToListAsync(ct);
        var profiles = await identityService.GetProfilesAsync(candidateIds, ct);
        var filteredProfiles = profiles
            .Where(profile => profile.IsActive)
            .Where(profile => !query.MinRating.HasValue || profile.AverageRating >= query.MinRating.Value)
            .Where(profile => string.IsNullOrWhiteSpace(query.SearchTerm)
                || profile.FirstName.Contains(query.SearchTerm, StringComparison.OrdinalIgnoreCase)
                || profile.LastName.Contains(query.SearchTerm, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(profile => profile.AverageRating)
            .ThenBy(profile => profile.FirstName)
            .ToList();

        var totalCount = filteredProfiles.Count;
        var pageUserIds = filteredProfiles
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(profile => profile.UserId)
            .ToList();

        var skillsByUser = await context.UserSkills
            .AsNoTracking()
            .Where(userSkill => pageUserIds.Contains(userSkill.UserId))
            .OrderBy(userSkill => userSkill.Skill.Name)
            .Select(userSkill => new
            {
                userSkill.UserId,
                Skill = new UserSkillSummaryDto(
                    userSkill.SkillId,
                    userSkill.Skill.Name,
                    userSkill.Skill.Category.Name,
                    userSkill.Type,
                    userSkill.ProficiencyLevel)
            })
            .ToListAsync(ct);

        var skillsLookup = skillsByUser
            .GroupBy(item => item.UserId)
            .ToDictionary(group => group.Key, group => (IReadOnlyList<UserSkillSummaryDto>)group.Select(item => item.Skill).ToList());
        var items = filteredProfiles
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
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
