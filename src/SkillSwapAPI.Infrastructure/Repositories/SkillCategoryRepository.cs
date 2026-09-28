using Microsoft.EntityFrameworkCore;
using SkillSwapAPI.Application.Common.Interfaces.Repos;
using SkillSwapAPI.Domain.Skills.Entities;
using SkillSwapAPI.Infrastructure.Persistence.Data.DbContext;

namespace SkillSwapAPI.Infrastructure.Repositories
{
    public class SkillCategoryRepository(
        ApplicationDbContext context) : BaseRepository<SkillCategory>(context), ISkillCategoryRepository
    {
        private readonly ApplicationDbContext _context = context;

        public async Task<IEnumerable<SkillCategory>> GetActiveWithSkillsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Set<SkillCategory>()
                .Include(category => category.Skills)
                .Where(category => category.IsActive)
                .OrderBy(category => category.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            return await _context.Set<SkillCategory>().AnyAsync(
                category => category.Name == name,
                cancellationToken);
        }

        public async Task<bool> ExistsAsync(int categoryId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<SkillCategory>().AnyAsync(
                category => category.Id == categoryId,
                cancellationToken);
        }
    }
}
