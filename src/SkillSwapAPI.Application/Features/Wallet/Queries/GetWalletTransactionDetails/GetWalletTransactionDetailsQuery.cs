using MediatR;
using SkillSwapAPI.Application.Features.Wallet.Dtos;
using SkillSwapAPI.Domain.Common.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkillSwapAPI.Application.Features.Wallet.Queries.GetWalletTransactionDetails
{
    public sealed record GetWalletTransactionDetailsQuery(
       Guid UserId,
       Guid TransactionId
   ) : IRequest<Result<WalletTransactionDto>>;
}
