using Microsoft.EntityFrameworkCore;
using SkillSwapAPI.Application.Common.Interfaces.Repos;
using SkillSwapAPI.Domain.Modules.Badges.Entities;
using SkillSwapAPI.Infrastructure.Persistence.Data.DbContext;

namespace SkillSwapAPI.Infrastructure.Repositories
{
    public class BadgeRepository(
        ApplicationDbContext context) : BaseRepository<Badge>(context), IBadgeRepository
    {
        private readonly ApplicationDbContext _context = context;

        public async Task<Badge?> GetByIdAsync(int badgeId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<Badge>()
                .FirstOrDefaultAsync(badge => badge.Id == badgeId, cancellationToken);
        }

        public async Task<IReadOnlyList<Badge>> GetActiveAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Set<Badge>()
                .AsNoTracking()
                .Where(badge => badge.IsActive)
                .OrderBy(badge => badge.Id)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Badge>> GetByIdsAsync(
            IReadOnlyCollection<int> badgeIds,
            CancellationToken cancellationToken = default)
        {
            return await _context.Set<Badge>()
                .AsNoTracking()
                .Where(badge => badgeIds.Contains(badge.Id))
                .ToListAsync(cancellationToken);
        }
    }
}
