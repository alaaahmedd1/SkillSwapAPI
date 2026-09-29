using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkillSwapAPI.Application.Features.Wallet.Queries.GetWalletTransactionReceipt
{
    public sealed record GetWalletTransactionReceiptQuery(
      Guid TransactionId)
      : IRequest<byte[]>;
}
