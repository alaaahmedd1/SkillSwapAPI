using SkillSwapAPI.Domain.Modules.SwapRequests.Entities;
using SkillSwapAPI.Domain.Modules.SwapRequests.Enums;

namespace SkillSwapAPI.Application.Common.Interfaces.Repos
{
    public interface ISwapRequestRepository : IBaseRepository<SwapRequest>
    {
        Task<SwapRequest?> GetByIdWithDetailsAsync(Guid swapRequestId, CancellationToken cancellationToken = default);

        Task<(IReadOnlyList<SwapRequest> Items, int TotalCount)> GetPagedForUserAsync(
            Guid userId,
            SwapRequestStatus? status,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);

        Task<bool> HasPendingDuplicateAsync(
            Guid requesterId,
            Guid receiverId,
            Guid offeredSkillId,
            Guid requestedSkillId,
            CancellationToken cancellationToken = default);
    }
}
