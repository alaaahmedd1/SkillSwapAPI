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
    public sealed class TimeLedgerTransactionRepository
           : BaseRepository<TimeLedgerTransaction>,
             ITimeLedgerTransactionRepository
    {
        private readonly ApplicationDbContext _context;

        public TimeLedgerTransactionRepository(
            ApplicationDbContext context)
            : base(context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<TimeLedgerTransaction>>
            GetByWalletIdAsync(
                Guid walletId,
                CancellationToken cancellationToken = default)
        {
            return await _context.TimeLedgerTransactions
                .AsNoTracking()
                .Where(transaction => transaction.WalletId == walletId)
                .OrderByDescending(transaction => transaction.CreatedAtUtc)
                .ToListAsync(cancellationToken);
        }
    }
}
