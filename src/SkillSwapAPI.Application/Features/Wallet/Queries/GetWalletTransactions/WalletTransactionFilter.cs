using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkillSwapAPI.Application.Features.Wallet.Queries.GetWalletTransactions
{
    public enum WalletTransactionFilter
    {
        All = 0,
        Earned = 1,
        Spent = 2
    }
}
