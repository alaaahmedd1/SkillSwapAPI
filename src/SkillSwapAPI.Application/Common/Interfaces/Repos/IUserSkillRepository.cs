using SkillSwapAPI.Domain.Modules.Users.Entities;
using SkillSwapAPI.Domain.Modules.Users.Enums;

namespace SkillSwapAPI.Application.Common.Interfaces.Repos
{
    public interface IUserSkillRepository : IBaseRepository<UserSkill>
    {
        Task<UserSkill?> GetByIdAndUserAsync(Guid userSkillId, Guid userId, CancellationToken cancellationToken = default);

        Task<IEnumerable<UserSkill>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);

        Task<IEnumerable<UserSkill>> GetByUserIdsAsync(IReadOnlyCollection<Guid> userIds, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Guid>> GetUserIdsByFiltersAsync(
            int? categoryId,
            Guid? offeredSkillId,
            Guid? seekingSkillId,
            ProficiencyLevel? proficiencyLevel,
            CancellationToken cancellationToken = default);

        Task<bool> HasSkillAsync(Guid userId, Guid skillId, SkillType type, CancellationToken cancellationToken = default);

        Task<bool> HasSkillWithDifferentTypeAsync(Guid userId, Guid skillId, SkillType type, CancellationToken cancellationToken = default);
    }
}
