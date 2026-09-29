using Microsoft.EntityFrameworkCore;
using SkillSwapAPI.Application.Common.Interfaces.Repos;
using SkillSwapAPI.Domain.Modules.Badges.Entities;
using SkillSwapAPI.Infrastructure.Persistence.Data.DbContext;

namespace SkillSwapAPI.Infrastructure.Repositories
{
    public class UserBadgeAwardRepository(
        ApplicationDbContext context) : BaseRepository<UserBadgeAward>(context), IUserBadgeAwardRepository
    {
        private readonly ApplicationDbContext _context = context;

        public async Task<IReadOnlyList<UserBadgeAward>> GetByRevieweeAsync(
            Guid revieweeId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Set<UserBadgeAward>()
                .AsNoTracking()
                .Where(award => award.RevieweeId == revieweeId)
                .OrderByDescending(award => award.AwardedAtUtc)
                .ToListAsync(cancellationToken);
        }
    }
}
