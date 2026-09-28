using Microsoft.EntityFrameworkCore;
using SkillSwapAPI.Application.Common.Interfaces.Repos;
using SkillSwapAPI.Domain.Identity;
using SkillSwapAPI.Infrastructure.Persistence.Data.DbContext;

namespace SkillSwapAPI.Infrastructure.Repositories
{
    public class RefreshTokenRepository(
        ApplicationDbContext context) : BaseRepository<RefreshToken>(context), IRefreshTokenRepository
    {
        private readonly ApplicationDbContext _context = context;

        public async Task<RefreshToken?> GetByUserAndTokenAsync(string userId, string tokenHash, CancellationToken cancellationToken = default)
        {
            return await _context.Set<RefreshToken>().FirstOrDefaultAsync(
                token => token.UserId == userId && token.Token == tokenHash,
                cancellationToken);
        }

        public async Task<RefreshToken?> GetActiveByUserAndTokenAsync(string userId, string tokenHash, CancellationToken cancellationToken = default)
        {
            return await _context.Set<RefreshToken>().FirstOrDefaultAsync(
                token => token.UserId == userId && token.Token == tokenHash && !token.IsRevoked,
                cancellationToken);
        }

        public async Task<IEnumerable<RefreshToken>> GetActiveByUserAsync(string userId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<RefreshToken>()
                .Where(token => token.UserId == userId && !token.IsRevoked)
                .ToListAsync(cancellationToken);
        }
    }
}
