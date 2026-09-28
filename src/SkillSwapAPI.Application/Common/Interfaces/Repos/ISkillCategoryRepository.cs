using SkillSwapAPI.Domain.Skills.Entities;

namespace SkillSwapAPI.Application.Common.Interfaces.Repos
{
    public interface ISkillCategoryRepository : IBaseRepository<SkillCategory>
    {
        Task<IEnumerable<SkillCategory>> GetActiveWithSkillsAsync(CancellationToken cancellationToken = default);

        Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);

        Task<bool> ExistsAsync(int categoryId, CancellationToken cancellationToken = default);
    }
}
