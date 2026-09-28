using SkillSwapAPI.Application.Common.Interfaces.Repos;
using SkillSwapAPI.Domain.Modules.Payments.Entities;
using SkillSwapAPI.Infrastructure.Persistence.Data.DbContext;
using Microsoft.EntityFrameworkCore;

namespace SkillSwapAPI.Infrastructure.Repositories;

public class CreditPackageRepository : BaseRepository<CreditPackage>, ICreditPackageRepository
{
    private readonly ApplicationDbContext _context;

    public CreditPackageRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<CreditPackage>> GetActivePackagesAsync(CancellationToken ct = default)
    {
        return await _context.CreditPackages
            .AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.Price)
            .ToListAsync(ct);
    }
}
