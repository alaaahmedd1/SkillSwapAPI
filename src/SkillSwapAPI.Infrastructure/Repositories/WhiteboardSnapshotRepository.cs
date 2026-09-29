using Microsoft.EntityFrameworkCore;
using SkillSwapAPI.Application.Common.Interfaces.Repos;
using SkillSwapAPI.Domain.Modules.LiveSessions.Entities;
using SkillSwapAPI.Infrastructure.Persistence.Data.DbContext;

namespace SkillSwapAPI.Infrastructure.Repositories
{
    public class WhiteboardSnapshotRepository(
        ApplicationDbContext context) : BaseRepository<WhiteboardSnapshot>(context), IWhiteboardSnapshotRepository
    {
        private readonly ApplicationDbContext _context = context;

        public async Task<WhiteboardSnapshot?> GetByRoomIdAsync(Guid roomId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<WhiteboardSnapshot>()
                .AsNoTracking()
                .FirstOrDefaultAsync(snapshot => snapshot.RoomId == roomId, cancellationToken);
        }
    }
}
