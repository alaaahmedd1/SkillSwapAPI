using MediatR;
using SkillSwapAPI.Application.Common.Errors;
using SkillSwapAPI.Application.Common.Interfaces.UnitOfWork;
using SkillSwapAPI.Application.Features.Wallet.Dtos;
using SkillSwapAPI.Domain.Common.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkillSwapAPI.Application.Features.Wallet.Queries.GetWalletTransactionDetails
{

    public sealed class GetWalletTransactionDetailsQueryHandler
        : IRequestHandler<
            GetWalletTransactionDetailsQuery,
            Result<WalletTransactionDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetWalletTransactionDetailsQueryHandler(
            IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<WalletTransactionDto>> Handle(
            GetWalletTransactionDetailsQuery request,
            CancellationToken cancellationToken)
        {
            var userId = request.UserId;

            var wallet = await _unitOfWork.TimeWallets
                .GetByUserIdAsync(userId, cancellationToken);

            if (wallet is null)
            {
                return ApplicationErrors.Wallet.WalletNotFound;
            }

            var transaction = await _unitOfWork
                .TimeLedgerTransactions
                .GetByIdAsync(request.TransactionId);

            if (transaction is null ||
                transaction.WalletId != wallet.Id)
            {
                return ApplicationErrors.Wallet.TransactionNotFound;
            }

            return new WalletTransactionDto(
                transaction.Id,
                transaction.ReferenceCode,
                transaction.Title,
                transaction.TransactionType.ToString(),
                transaction.AmountMinutes,
                transaction.RunningBalanceMinutes,
                transaction.SwapRequestId,
                transaction.PartnerUserId,
                transaction.CreatedAtUtc);
        }
    }
    }
