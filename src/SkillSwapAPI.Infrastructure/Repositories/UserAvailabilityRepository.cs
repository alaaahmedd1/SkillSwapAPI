using Microsoft.EntityFrameworkCore;
using SkillSwapAPI.Application.Common.Interfaces.Repos;
using SkillSwapAPI.Domain.Modules.Users.Entities;
using SkillSwapAPI.Infrastructure.Persistence.Data.DbContext;

namespace SkillSwapAPI.Infrastructure.Repositories;

public class UserAvailabilityRepository(
    ApplicationDbContext context) : BaseRepository<UserAvailability>(context), IUserAvailabilityRepository
{
    private readonly ApplicationDbContext _context = context;

    public async Task<IReadOnlyList<UserAvailability>> GetByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Set<UserAvailability>()
            .AsNoTracking()
            .Where(availability => availability.UserId == userId)
            .OrderBy(availability => availability.DayOfWeek)
            .ThenBy(availability => availability.TimeBlock)
            .ToListAsync(cancellationToken);
    }

    public async Task ReplaceForUserAsync(
        Guid userId,
        IEnumerable<UserAvailability> availability,
        CancellationToken cancellationToken = default)
    {
        var existing = await _context.Set<UserAvailability>()
            .Where(row => row.UserId == userId)
            .ToListAsync(cancellationToken);

        _context.Set<UserAvailability>().RemoveRange(existing);
        _context.Set<UserAvailability>().AddRange(availability);

        await Task.CompletedTask;
    }
}
