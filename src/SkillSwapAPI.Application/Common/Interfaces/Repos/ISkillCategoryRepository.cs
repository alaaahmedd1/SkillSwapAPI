using SkillSwapAPI.Domain.Skills.Entities;

namespace SkillSwapAPI.Application.Common.Interfaces.Repos
{
    public interface ISkillCategoryRepository : IBaseRepository<SkillCategory>
    {
        Task<IEnumerable<SkillCategory>> GetActiveWithSkillsAsync(CancellationToken cancellationToken = default);
    }
}
