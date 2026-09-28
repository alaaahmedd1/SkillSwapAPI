using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SkillSwapAPI.Application.Features.Wallet.Queries.GetMyWallet;

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
    }
}
