using MediatR;
using SkillSwapAPI.Application.Common.Errors;
using SkillSwapAPI.Application.Common.Interfaces.Services;
using SkillSwapAPI.Application.Common.Interfaces.UnitOfWork;
using SkillSwapAPI.Domain.Common.Results;


namespace SkillSwapAPI.Application.Features.Wallet.Queries.GetWalletTransactionReceipt
{
    public sealed class GetWalletTransactionReceiptQueryHandler
          : IRequestHandler<GetWalletTransactionReceiptQuery, Result<byte[]>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPdfService _pdfService;
        public GetWalletTransactionReceiptQueryHandler(
            IUnitOfWork unitOfWork,
            IPdfService pdfService)
        {
            _unitOfWork = unitOfWork;
            _pdfService = pdfService;
        }

        public async Task<Result<byte[]>> Handle(
      GetWalletTransactionReceiptQuery request,
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
