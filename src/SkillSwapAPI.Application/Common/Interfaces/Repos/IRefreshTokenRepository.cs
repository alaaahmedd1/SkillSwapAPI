using SkillSwapAPI.Domain.Identity;

namespace SkillSwapAPI.Application.Common.Interfaces.Repos
{
    public interface IRefreshTokenRepository : IBaseRepository<RefreshToken>
    {
        Task<RefreshToken?> GetByUserAndTokenAsync(string userId, string tokenHash, CancellationToken cancellationToken = default);

        Task<RefreshToken?> GetActiveByUserAndTokenAsync(string userId, string tokenHash, CancellationToken cancellationToken = default);

        Task<IEnumerable<RefreshToken>> GetActiveByUserAsync(string userId, CancellationToken cancellationToken = default);
    }
}
