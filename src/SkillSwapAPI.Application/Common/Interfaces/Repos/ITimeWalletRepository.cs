using SkillSwapAPI.Domain.Modules.Wallet.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkillSwapAPI.Application.Common.Interfaces.Repos
{
    public interface ITimeWalletRepository : IBaseRepository<TimeWallet>
    {
        Task<TimeWallet?> GetByUserIdAsync(
            Guid userId,
            CancellationToken cancellationToken = default);
    }
}
