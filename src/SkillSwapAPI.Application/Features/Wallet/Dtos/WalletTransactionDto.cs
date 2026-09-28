using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkillSwapAPI.Application.Features.Wallet.Dtos
{
    public sealed record WalletTransactionDto(
        Guid Id,
        string ReferenceCode,
        string Title,
        string TransactionType,
        int AmountMinutes,
        int RunningBalanceMinutes,
        Guid? SwapRequestId,
        Guid? PartnerUserId,
        DateTimeOffset CreatedAtUtc);
}
