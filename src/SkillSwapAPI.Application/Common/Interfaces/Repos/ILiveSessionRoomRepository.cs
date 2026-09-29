using SkillSwapAPI.Domain.Modules.LiveSessions.Entities;

namespace SkillSwapAPI.Application.Common.Interfaces.Repos;

public interface ILiveSessionRoomRepository : IBaseRepository<LiveSessionRoom>
{
    Task<LiveSessionRoom?> GetBySwapRequestIdAsync(
        Guid swapRequestId,
        CancellationToken cancellationToken = default);
}
