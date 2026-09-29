using SkillSwapAPI.Domain.Modules.Badges.Entities;

namespace SkillSwapAPI.Application.Common.Interfaces.Repos
{
    public interface IUserBadgeAwardRepository : IBaseRepository<UserBadgeAward>
    {
        Task<IReadOnlyList<UserBadgeAward>> GetByRevieweeAsync(
            Guid revieweeId,
            CancellationToken cancellationToken = default);
    }
}
