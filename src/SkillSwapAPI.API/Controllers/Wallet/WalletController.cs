using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SkillSwapAPI.Application.Features.Wallet.Queries.GetMyWallet;
using SkillSwapAPI.Application.Features.Wallet.Queries.GetWalletTransactionReceipt;
using SkillSwapAPI.Application.Features.Wallet.Queries.GetWalletTransactions;

namespace SkillSwapAPI.API.Controllers.Wallet
{
    [Authorize]
    [ApiController]
    [Route("api/v1/wallet")]
    public sealed class WalletController : ApiBaseController
    {
        private readonly ISender _sender;

        public WalletController(ISender sender)
        {
            _sender = sender;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMyWallet(
            CancellationToken cancellationToken)
        {
            var result = await _sender.Send(
                new GetMyWalletQuery(),
                cancellationToken);

            return Ok(result);
        }

        [HttpGet("transactions")]
        public async Task<IActionResult> GetTransactions(
            [FromQuery] GetWalletTransactionsQuery query,
            CancellationToken cancellationToken)
        {
            var result = await _sender.Send(
                query,
                cancellationToken);

            return Ok(result);
        }


        [HttpGet("transactions/{id}/receipt")]
        public async Task<IActionResult> GetTransactionReceipt(
            Guid id,
            CancellationToken cancellationToken)
        {
            var pdf = await _sender.Send(
                new GetWalletTransactionReceiptQuery(id),
                cancellationToken);

            return File(
                pdf,
                "application/pdf",
                $"time-receipt-{id}.pdf");
        }
    }
}
