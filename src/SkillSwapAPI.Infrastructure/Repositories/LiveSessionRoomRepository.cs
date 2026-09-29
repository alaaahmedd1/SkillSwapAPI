using Microsoft.EntityFrameworkCore;
using SkillSwapAPI.Application.Common.Interfaces.Repos;
using SkillSwapAPI.Domain.Modules.LiveSessions.Entities;
using SkillSwapAPI.Infrastructure.Persistence.Data.DbContext;

namespace SkillSwapAPI.Infrastructure.Repositories;

public class LiveSessionRoomRepository(
    ApplicationDbContext context) : BaseRepository<LiveSessionRoom>(context), ILiveSessionRoomRepository
{
    private readonly ApplicationDbContext _context = context;

    public async Task<LiveSessionRoom?> GetBySwapRequestIdAsync(
        Guid swapRequestId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<LiveSessionRoom>()
            .FirstOrDefaultAsync(room => room.SwapRequestId == swapRequestId, cancellationToken);
    }
}
