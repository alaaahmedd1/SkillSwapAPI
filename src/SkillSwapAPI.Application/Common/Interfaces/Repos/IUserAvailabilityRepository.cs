using SkillSwapAPI.Application.Common.Interfaces.Repos;
using SkillSwapAPI.Domain.Modules.Users.Entities;

namespace SkillSwapAPI.Application.Common.Interfaces.Repos;

public interface IUserAvailabilityRepository : IBaseRepository<UserAvailability>
{
    Task<IReadOnlyList<UserAvailability>> GetByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    Task ReplaceForUserAsync(
        Guid userId,
        IEnumerable<UserAvailability> availability,
        CancellationToken cancellationToken = default);
}
