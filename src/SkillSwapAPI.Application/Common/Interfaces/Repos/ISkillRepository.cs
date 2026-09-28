using SkillSwapAPI.Domain.Skills.Entities;

namespace SkillSwapAPI.Application.Common.Interfaces.Repos
{
    public interface ISkillRepository : IBaseRepository<Skill>
    {
        Task<Skill?> GetWithCategoryAsync(Guid skillId, CancellationToken cancellationToken = default);

        Task<IEnumerable<Skill>> GetByIdsWithCategoryAsync(IReadOnlyCollection<Guid> skillIds, CancellationToken cancellationToken = default);

        Task<bool> ExistsAsync(Guid skillId, CancellationToken cancellationToken = default);

        Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);
    }
}
