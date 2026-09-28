using Microsoft.EntityFrameworkCore;
using SkillSwapAPI.Application.Common.Interfaces.Repos;
using SkillSwapAPI.Domain.Skills.Entities;
using SkillSwapAPI.Infrastructure.Persistence.Data.DbContext;

namespace SkillSwapAPI.Infrastructure.Repositories
{
    public class SkillRepository(
        ApplicationDbContext context) : BaseRepository<Skill>(context), ISkillRepository
    {
        private readonly ApplicationDbContext _context = context;

        public async Task<Skill?> GetWithCategoryAsync(Guid skillId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<Skill>()
                .Include(skill => skill.Category)
                .FirstOrDefaultAsync(skill => skill.Id == skillId, cancellationToken);
        }

        public async Task<IEnumerable<Skill>> GetByIdsWithCategoryAsync(IReadOnlyCollection<Guid> skillIds, CancellationToken cancellationToken = default)
        {
            return await _context.Set<Skill>()
                .Include(skill => skill.Category)
                .Where(skill => skillIds.Contains(skill.Id))
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> ExistsAsync(Guid skillId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<Skill>().AnyAsync(skill => skill.Id == skillId, cancellationToken);
        }
    }
}
