using MediatR;
using SkillSwapAPI.Application.Common.Models;
using SkillSwapAPI.Application.Features.Wallet.Dtos;


namespace SkillSwapAPI.Application.Features.Wallet.Queries.GetWalletTransactions
{
    public sealed record GetWalletTransactionsQuery(Guid UserId
)
        : PagedRequest,
          IRequest<PagedResult<WalletTransactionDto>>
    {
        public WalletTransactionFilter Filter { get; init; }
            = WalletTransactionFilter.All;
    }
}
