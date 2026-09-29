using MediatR;
using SkillSwapAPI.Application.Common.Interfaces.Identity;
using SkillSwapAPI.Application.Common.Interfaces.Services;
using SkillSwapAPI.Application.Common.Interfaces.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkillSwapAPI.Application.Features.Wallet.Queries.GetWalletTransactionReceipt
{
    public sealed class GetWalletTransactionReceiptQueryHandler
          : IRequestHandler<GetWalletTransactionReceiptQuery, byte[]>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPdfService _pdfService;
        private readonly IUser _currentUser;
        public GetWalletTransactionReceiptQueryHandler(
            IUnitOfWork unitOfWork,
            IPdfService pdfService,
            IUser currentUser)
        {
            _unitOfWork = unitOfWork;
            _pdfService = pdfService;
            _currentUser = currentUser;
        }

        public async Task<byte[]> Handle(
      GetWalletTransactionReceiptQuery request,
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

            return _pdfService.GenerateTimeReceipt(
                transaction.Id,
                transaction.ReferenceCode,
                transaction.Title,
                transaction.TransactionType.ToString(),
                transaction.AmountMinutes,
                transaction.RunningBalanceMinutes,
                transaction.CreatedAtUtc);
        }
    }
}
