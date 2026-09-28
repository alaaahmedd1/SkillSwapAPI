using MediatR;
using SkillSwapAPI.Application.Common.Interfaces.Identity;
using SkillSwapAPI.Application.Common.Interfaces.UnitOfWork;
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
    public sealed class GetWalletTransactionsQueryHandler
        : IRequestHandler<
            GetWalletTransactionsQuery,
            PagedResult<WalletTransactionDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUser _currentUser;

        public GetWalletTransactionsQueryHandler(
            IUnitOfWork unitOfWork,
            IUser currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<PagedResult<WalletTransactionDto>> Handle(
            GetWalletTransactionsQuery request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUser.Id;

            var wallet = await _unitOfWork.TimeWallets
                .GetByUserIdAsync(userId, cancellationToken);

            if (wallet is null)
            {
                return PagedResult<WalletTransactionDto>.Create(
                    [],
                    0,
                    request.PageNumber,
                    request.PageSize);
            }

            var transactions = await _unitOfWork.TimeLedgerTransactions
                .GetByWalletIdAsync(
                    wallet.Id,
                    cancellationToken);

            var filteredTransactions = request.Filter switch
            {
                WalletTransactionFilter.Earned =>
                    transactions.Where(
                        t => t.TransactionType == TransactionType.Earned),

                WalletTransactionFilter.Spent =>
                    transactions.Where(
                        t => t.TransactionType == TransactionType.Spent),

                _ => transactions
            };

            var totalCount = filteredTransactions.Count();

            var items = filteredTransactions
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(transaction => new WalletTransactionDto(
                    transaction.Id,
                    transaction.ReferenceCode,
                    transaction.Title,
                    transaction.TransactionType.ToString(),
                    transaction.AmountMinutes,
                    transaction.RunningBalanceMinutes,
                    transaction.SwapRequestId,
                    transaction.PartnerUserId,
                    transaction.CreatedAtUtc))
                .ToList();

            return PagedResult<WalletTransactionDto>.Create(
                items,
                totalCount,
                request.PageNumber,
                request.PageSize);
        }
    }

}
