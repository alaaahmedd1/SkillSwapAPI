using Microsoft.EntityFrameworkCore;
using SkillSwapAPI.Application.Common.Interfaces.Repos;
using SkillSwapAPI.Domain.Modules.Wallet.Entities;
using SkillSwapAPI.Infrastructure.Persistence.Data.DbContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkillSwapAPI.Infrastructure.Repositories
{
    public sealed class TimeWalletRepository
     : BaseRepository<TimeWallet>, ITimeWalletRepository
    {
        private readonly ApplicationDbContext _context;

        public TimeWalletRepository(ApplicationDbContext context)
            : base(context)
        {
            _context = context;
        }

        public async Task<TimeWallet?> GetByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            return await _context.TimeWallets
                .FirstOrDefaultAsync(
                    wallet => wallet.UserId == userId,
                    cancellationToken);
        }
    }
    }
