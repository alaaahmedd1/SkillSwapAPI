using SkillSwapAPI.Domain.Modules.Badges.Entities;

namespace SkillSwapAPI.Application.Common.Interfaces.Repos
{
    public interface IBadgeRepository : IBaseRepository<Badge>
    {
        Task<Badge?> GetByIdAsync(int badgeId, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Badge>> GetActiveAsync(CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Badge>> GetByIdsAsync(
            IReadOnlyCollection<int> badgeIds,
            CancellationToken cancellationToken = default);
    }
}
