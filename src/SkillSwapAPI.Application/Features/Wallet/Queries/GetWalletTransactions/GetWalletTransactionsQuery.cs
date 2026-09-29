using MediatR;
using SkillSwapAPI.Application.Common.Models;
using SkillSwapAPI.Application.Features.Wallet.Dtos;
using SkillSwapAPI.Domain.Modules.Wallet.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkillSwapAPI.Application.Features.Wallet.Queries.GetWalletTransactions
{
    public sealed record GetWalletTransactionsQuery
        : PagedRequest,
          IRequest<PagedResult<WalletTransactionDto>>
    {
        public WalletTransactionFilter Filter { get; init; }
            = WalletTransactionFilter.All;
    }
}
