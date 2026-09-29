using MediatR;
using SkillSwapAPI.Application.Common.Interfaces.Identity;
using SkillSwapAPI.Application.Common.Interfaces.UnitOfWork;
using SkillSwapAPI.Application.Features.Wallet.Dtos;
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
            WalletTransactionDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUser _currentUser;

        public GetWalletTransactionDetailsQueryHandler(
            IUnitOfWork unitOfWork,
            IUser currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<WalletTransactionDto> Handle(
            GetWalletTransactionDetailsQuery request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUser.Id;

            var wallet = await _unitOfWork.TimeWallets
                .GetByUserIdAsync(userId, cancellationToken);

            if (wallet is null)
            {
                throw new KeyNotFoundException(
                    "Wallet was not found.");
            }

            var transaction = await _unitOfWork
                .TimeLedgerTransactions
                .GetByIdAsync(request.TransactionId);

            if (transaction is null ||
                transaction.WalletId != wallet.Id)
            {
                throw new KeyNotFoundException(
                    "Wallet transaction was not found.");
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
