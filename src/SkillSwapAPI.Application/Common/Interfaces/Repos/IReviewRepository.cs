using SkillSwapAPI.Domain.Modules.Reviews.Entities;

namespace SkillSwapAPI.Application.Common.Interfaces.Repos
{
    public interface IReviewRepository : IBaseRepository<Review>
    {
        Task<bool> HasReviewAsync(
            Guid swapRequestId,
            Guid reviewerId,
            CancellationToken cancellationToken = default);

        Task<(IReadOnlyList<Review> Items, int TotalCount)> GetPagedByRevieweeAsync(
            Guid revieweeId,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);

        Task<(decimal AverageRating, int TotalReviewsCount)> GetRatingSummaryAsync(
            Guid revieweeId,
            CancellationToken cancellationToken = default);
    }
}
