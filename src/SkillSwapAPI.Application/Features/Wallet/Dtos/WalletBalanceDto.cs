using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkillSwapAPI.Application.Features.Wallet.Dtos
{
    public sealed record WalletBalanceDto(
      int BalanceMinutes,
      int TotalEarnedMinutes,
      int TotalSpentMinutes);
}
