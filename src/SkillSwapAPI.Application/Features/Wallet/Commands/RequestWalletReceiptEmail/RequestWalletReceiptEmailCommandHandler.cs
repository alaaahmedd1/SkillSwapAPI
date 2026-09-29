using Hangfire;
using MediatR;
using SkillSwapAPI.Application.Common.Errors;
using SkillSwapAPI.Application.Common.Interfaces.Services;
using SkillSwapAPI.Application.Common.Interfaces.UnitOfWork;
using SkillSwapAPI.Domain.Common.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkillSwapAPI.Application.Features.Wallet.Commands.RequestWalletReceiptEmail
{

    public sealed class RequestWalletReceiptEmailCommandHandler(
        IUnitOfWork unitOfWork,
        IBackgroundJobClient backgroundJobClient)
        : IRequestHandler<
            RequestWalletReceiptEmailCommand,
            Result<Success>>
    {
        public async Task<Result<Success>> Handle(
            RequestWalletReceiptEmailCommand request,
            CancellationToken cancellationToken)
        {
            var wallet = await unitOfWork.TimeWallets
                .GetByUserIdAsync(
                    request.UserId,
                    cancellationToken);

            if (wallet is null)
            {
                return ApplicationErrors.Wallet.WalletNotFound;
            }

            var transaction = await unitOfWork.TimeLedgerTransactions
                .GetByIdAsync(request.TransactionId);

            if (transaction is null ||
                transaction.WalletId != wallet.Id)
            {
                return ApplicationErrors.Wallet.TransactionNotFound;
            }

            backgroundJobClient.Enqueue<IWalletReceiptJob>(
                job => job.SendReceiptAsync(
                    request.UserId,
                    request.TransactionId,
                    CancellationToken.None));

            return Result.Success;
        }}
    }
