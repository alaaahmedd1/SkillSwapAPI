using Microsoft.EntityFrameworkCore;
using SkillSwapAPI.Application.Common.Interfaces.Repos;
using SkillSwapAPI.Domain.Modules.SwapRequests.Entities;
using SkillSwapAPI.Domain.Modules.SwapRequests.Enums;
using SkillSwapAPI.Infrastructure.Persistence.Data.DbContext;

namespace SkillSwapAPI.Infrastructure.Repositories
{
    public class SwapRequestRepository(
        ApplicationDbContext context) : BaseRepository<SwapRequest>(context), ISwapRequestRepository
    {
        private readonly ApplicationDbContext _context = context;

        public async Task<SwapRequest?> GetByIdWithDetailsAsync(Guid swapRequestId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<SwapRequest>()
                .Include(swapRequest => swapRequest.OfferedSkill.Category)
                .Include(swapRequest => swapRequest.RequestedSkill.Category)
                .FirstOrDefaultAsync(swapRequest => swapRequest.Id == swapRequestId, cancellationToken);
        }

        public async Task<(IReadOnlyList<SwapRequest> Items, int TotalCount)> GetPagedForUserAsync(
            Guid userId,
            SwapRequestStatus? status,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Set<SwapRequest>()
                .Include(swapRequest => swapRequest.OfferedSkill.Category)
                .Include(swapRequest => swapRequest.RequestedSkill.Category)
                .Where(swapRequest => swapRequest.RequesterId == userId || swapRequest.ReceiverId == userId);

            if (status.HasValue)
            {
                query = query.Where(swapRequest => swapRequest.Status == status.Value);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(swapRequest => swapRequest.CreatedAtUtc)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        public async Task<bool> HasPendingDuplicateAsync(
            Guid requesterId,
            Guid receiverId,
            Guid offeredSkillId,
            Guid requestedSkillId,
            CancellationToken cancellationToken = default)
        {
            return await _context.Set<SwapRequest>().AnyAsync(
                swapRequest => swapRequest.Status == SwapRequestStatus.Pending
                    && swapRequest.RequesterId == requesterId
                    && swapRequest.ReceiverId == receiverId
                    && swapRequest.OfferedSkillId == offeredSkillId
                    && swapRequest.RequestedSkillId == requestedSkillId,
                cancellationToken);
        }
    }
}
