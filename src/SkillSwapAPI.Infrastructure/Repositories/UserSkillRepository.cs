using Microsoft.EntityFrameworkCore;
using SkillSwapAPI.Application.Common.Interfaces.Repos;
using SkillSwapAPI.Domain.Modules.Users.Entities;
using SkillSwapAPI.Domain.Modules.Users.Enums;
using SkillSwapAPI.Infrastructure.Persistence.Data.DbContext;

namespace SkillSwapAPI.Infrastructure.Repositories
{
    public class UserSkillRepository(
        ApplicationDbContext context) : BaseRepository<UserSkill>(context), IUserSkillRepository
    {
        private readonly ApplicationDbContext _context = context;

        public async Task<UserSkill?> GetByIdAndUserAsync(Guid userSkillId, Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<UserSkill>().FirstOrDefaultAsync(
                userSkill => userSkill.Id == userSkillId && userSkill.UserId == userId,
                cancellationToken);
        }

        public async Task<IEnumerable<UserSkill>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.Set<UserSkill>()
                .Include(userSkill => userSkill.Skill.Category)
                .Where(userSkill => userSkill.UserId == userId)
                .OrderBy(userSkill => userSkill.Skill.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<UserSkill>> GetByUserIdsAsync(IReadOnlyCollection<Guid> userIds, CancellationToken cancellationToken = default)
        {
            return await _context.Set<UserSkill>()
                .Include(userSkill => userSkill.Skill.Category)
                .Where(userSkill => userIds.Contains(userSkill.UserId))
                .OrderBy(userSkill => userSkill.Skill.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<Guid>> GetUserIdsByFiltersAsync(
            int? categoryId,
            Guid? offeredSkillId,
            Guid? seekingSkillId,
            ProficiencyLevel? proficiencyLevel,
            CancellationToken cancellationToken = default)
        {
            var userSkills = _context.Set<UserSkill>().AsNoTracking();
            var query = userSkills.Where(userSkill => userSkill.Type == SkillType.Offered);

            if (categoryId.HasValue)
            {
                query = query.Where(userSkill => userSkills.Any(
                    item => item.UserId == userSkill.UserId && item.Skill.CategoryId == categoryId.Value));
            }

            if (offeredSkillId.HasValue)
            {
                query = query.Where(userSkill => userSkill.SkillId == offeredSkillId.Value);
            }

            if (seekingSkillId.HasValue)
            {
                query = query.Where(userSkill => userSkills.Any(
                    item => item.UserId == userSkill.UserId
                        && item.Type == SkillType.Seeking
                        && item.SkillId == seekingSkillId.Value));
            }

            if (proficiencyLevel.HasValue)
            {
                query = query.Where(userSkill => userSkills.Any(
                    item => item.UserId == userSkill.UserId && item.ProficiencyLevel == proficiencyLevel.Value));
            }

            return await query
                .Select(userSkill => userSkill.UserId)
                .Distinct()
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> HasSkillAsync(Guid userId, Guid skillId, SkillType type, CancellationToken cancellationToken = default)
        {
            return await _context.Set<UserSkill>().AnyAsync(
                userSkill => userSkill.UserId == userId && userSkill.SkillId == skillId && userSkill.Type == type,
                cancellationToken);
        }

        public async Task<bool> HasSkillWithDifferentTypeAsync(Guid userId, Guid skillId, SkillType type, CancellationToken cancellationToken = default)
        {
            return await _context.Set<UserSkill>().AnyAsync(
                userSkill => userSkill.UserId == userId && userSkill.SkillId == skillId && userSkill.Type != type,
                cancellationToken);
        }
    }
}
