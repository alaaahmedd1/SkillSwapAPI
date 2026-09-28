using Microsoft.EntityFrameworkCore;
using SkillSwapAPI.Application.Common.Interfaces.Repos;
using SkillSwapAPI.Domain.Modules.Reviews.Entities;
using SkillSwapAPI.Infrastructure.Persistence.Data.DbContext;

namespace SkillSwapAPI.Infrastructure.Repositories
{
    public class ReviewRepository(
        ApplicationDbContext context) : BaseRepository<Review>(context), IReviewRepository
    {
        private readonly ApplicationDbContext _context = context;

        public async Task<bool> HasReviewAsync(
            Guid swapRequestId,
            Guid reviewerId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Set<Review>().AnyAsync(
                review => review.SwapRequestId == swapRequestId
                    && review.ReviewerId == reviewerId,
                cancellationToken);
        }

        public async Task<(IReadOnlyList<Review> Items, int TotalCount)> GetPagedByRevieweeAsync(
            Guid revieweeId,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Set<Review>()
                .AsNoTracking()
                .Where(review => review.RevieweeId == revieweeId);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(review => review.CreatedAtUtc)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task<(decimal AverageRating, int TotalReviewsCount)> GetRatingSummaryAsync(
            Guid revieweeId,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Set<Review>()
                .AsNoTracking()
                .Where(review => review.RevieweeId == revieweeId);

            var totalCount = await query.CountAsync(cancellationToken);

            if (totalCount == 0)
            {
                return (0.00m, 0);
            }

            var totalRating = await query.SumAsync(review => review.Rating, cancellationToken);

            return (Math.Round((decimal)totalRating / totalCount, 2), totalCount);
        }
    }
}
