using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillSwapAPI.Application.Features.Wallet.Queries.GetMyWallet;
using SkillSwapAPI.Application.Features.Wallet.Queries.GetWalletTransactionDetails;
using SkillSwapAPI.Application.Features.Wallet.Queries.GetWalletTransactionReceipt;
using SkillSwapAPI.Application.Features.Wallet.Queries.GetWalletTransactions;
using SkillSwapAPI.Domain.Common.Results;

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
            if (!TryGetCurrentUserId(out var userId))
            {
                return Unauthorized();
            }

            var result = await _sender.Send(
                new GetMyWalletQuery(userId),
                cancellationToken);

            return Ok(result);
        }

        [HttpGet("transactions")]
        public async Task<IActionResult> GetTransactions(
            [FromQuery] GetWalletTransactionsQuery query,
            CancellationToken cancellationToken)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Unauthorized();
            }

            var request = query with { UserId = userId };

            var result = await _sender.Send(
                request,
                cancellationToken);

            return Ok(result);
        }


        [HttpGet("transactions/{id:guid}")]
        public async Task<IActionResult> GetTransactionDetails(
         Guid id,
         CancellationToken cancellationToken)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Unauthorized();
            }

            var result = await _sender.Send(
                new GetWalletTransactionDetailsQuery(userId, id),
                cancellationToken);

            return HandleResult(result);
        }


        [HttpGet("transactions/{id:guid}/receipt")]
        public async Task<IActionResult> GetTransactionReceipt(
    Guid id,
    CancellationToken cancellationToken)
        {
            if (!TryGetCurrentUserId(out var userId))
            {
                return Unauthorized();
            }

            var result = await _sender.Send(
                new GetWalletTransactionReceiptQuery(userId, id),
                cancellationToken);

            if (!result.IsSuccess)
            {
                return HandleResult(result);
            }

            return File(
                result.Value,
                "application/pdf",
                $"wallet-transaction-{id}.pdf");
        }
    }
}
