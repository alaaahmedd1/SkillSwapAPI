using MediatR;
using SkillSwapAPI.Application.Common.Interfaces.Services;
using SkillSwapAPI.Application.Common.Interfaces.UnitOfWork;


namespace SkillSwapAPI.Application.Features.Wallet.Queries.GetWalletTransactionReceipt
{
    public sealed class GetWalletTransactionReceiptQueryHandler
          : IRequestHandler<GetWalletTransactionReceiptQuery, byte[]>
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

        public async Task<byte[]> Handle(
      GetWalletTransactionReceiptQuery request,
      CancellationToken cancellationToken)
        {
            var userId = request.UserId;

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
