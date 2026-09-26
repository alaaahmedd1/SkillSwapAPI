using Microsoft.EntityFrameworkCore;
using SkillSwapAPI.Domain.Identity;
using SkillSwapAPI.Domain.Skills.Entities;
using SkillSwapAPI.Domain.Modules.Users.Entities;
using System.Collections.Generic;

namespace SkillSwapAPI.Application.Common.Interfaces;

public interface IApplicationDbContext
{

    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<SkillCategory> SkillCategories { get; }
    DbSet<Skill> Skills { get; }
    DbSet<UserSkill> UserSkills { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
