using MediatR;
using SkillSwapAPI.Application.Common.Interfaces.Identity;
using SkillSwapAPI.Application.Common.Interfaces.UnitOfWork;
using SkillSwapAPI.Application.Features.Users.Dtos;
using SkillSwapAPI.Domain.Common.Results;

namespace SkillSwapAPI.Application.Features.Users.Queries.GetOwnProfile;

public sealed class GetOwnProfileQueryHandler(
    IUnitOfWork unitOfWork,
    IIdentityService identityService)
    : IRequestHandler<GetOwnProfileQuery, Result<ProfileDto>>
{
    public async Task<Result<ProfileDto>> Handle(GetOwnProfileQuery query, CancellationToken ct)
    {
        var userResult = await identityService.GetUserByIdAsync(query.UserId.ToString(), ct);
        if (userResult.IsError)
        {
            return userResult.Errors;
        }

        var userSkills = await unitOfWork.UserSkills.GetByUserAsync(query.UserId, ct);

        var skills = userSkills
            .Select(userSkill => new UserSkillDto(
                userSkill.Id,
                userSkill.SkillId,
                userSkill.Skill.Name,
                userSkill.Skill.Category.Name,
                userSkill.Type,
                userSkill.ProficiencyLevel,
                userSkill.YearsOfExperience))
            .ToList();

        return new ProfileDto(
            query.UserId,
            userResult.Value.FirstName,
            userResult.Value.LastName,
            userResult.Value.Email,
            userResult.Value.AverageRating,
            userResult.Value.TotalReviewsCount,
            userResult.Value.IsActive,
            userResult.Value.CreatedAtUtc,
            skills);
    }
}
