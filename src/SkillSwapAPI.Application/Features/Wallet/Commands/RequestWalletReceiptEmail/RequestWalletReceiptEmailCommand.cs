using MediatR;
using SkillSwapAPI.Domain.Common.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkillSwapAPI.Application.Features.Wallet.Commands.RequestWalletReceiptEmail
{
    public sealed record RequestWalletReceiptEmailCommand(
     Guid UserId,
     Guid TransactionId)
     : IRequest<Result<Success>>;
}
