using Microsoft.EntityFrameworkCore;
using SkillSwapAPI.Domain.Identity;
using System.Collections.Generic;

namespace SkillSwapAPI.Application.Common.Interfaces;

public interface IApplicationDbContext
{

    DbSet<RefreshToken> RefreshTokens { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
