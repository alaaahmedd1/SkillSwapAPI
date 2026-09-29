using MediatR;
using SkillSwapAPI.Application.Features.Wallet.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkillSwapAPI.Application.Features.Wallet.Queries.GetWalletTransactionDetails
{
    public sealed record GetWalletTransactionDetailsQuery(
        Guid TransactionId)
        : IRequest<WalletTransactionDto>;
}
