using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkillSwapAPI.Domain.Modules.Wallet.Enums
{
    public enum TransactionType
    {
        Earned = 1,
        Spent = 2,
        Purchased = 3,
        Refunded = 4
    }
}
