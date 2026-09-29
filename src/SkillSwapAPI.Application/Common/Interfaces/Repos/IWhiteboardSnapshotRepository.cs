using SkillSwapAPI.Domain.Modules.LiveSessions.Entities;

namespace SkillSwapAPI.Application.Common.Interfaces.Repos
{
    public interface IWhiteboardSnapshotRepository : IBaseRepository<WhiteboardSnapshot>
    {
        Task<WhiteboardSnapshot?> GetByRoomIdAsync(Guid roomId, CancellationToken cancellationToken = default);
    }
}
