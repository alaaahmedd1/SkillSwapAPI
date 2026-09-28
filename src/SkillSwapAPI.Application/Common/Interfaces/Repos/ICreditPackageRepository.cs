using SkillSwapAPI.Domain.Modules.Payments.Entities;

namespace SkillSwapAPI.Application.Common.Interfaces.Repos;

public interface ICreditPackageRepository : IBaseRepository<CreditPackage>
{
    Task<IReadOnlyList<CreditPackage>> GetActivePackagesAsync(CancellationToken ct = default);
}