using MediatR;
using SkillSwapAPI.Application.Common.Interfaces.Identity;
using SkillSwapAPI.Application.Common.Interfaces.UnitOfWork;
using SkillSwapAPI.Application.Features.Wallet.Dtos;
using SkillSwapAPI.Domain.Modules.Wallet.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkillSwapAPI.Application.Features.Wallet.Queries.GetMyWallet
{

    public sealed class GetMyWalletQueryHandler
        : IRequestHandler<GetMyWalletQuery, WalletBalanceDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUser _currentUser;

        public GetMyWalletQueryHandler(
            IUnitOfWork unitOfWork,
            IUser currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<WalletBalanceDto> Handle(
            GetMyWalletQuery request,
            CancellationToken cancellationToken)
        {
            var userId = _currentUser.Id;

            var wallet = await _unitOfWork.TimeWallets
                .GetByUserIdAsync(userId, cancellationToken);

            if (wallet is null)
            {
                wallet = new TimeWallet
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    BalanceMinutes = 0,
                    TotalEarnedMinutes = 0,
                    TotalSpentMinutes = 0,
                    CreatedAtUtc = DateTimeOffset.UtcNow
                };

                await _unitOfWork.TimeWallets.AddAsync(wallet);

                await _unitOfWork.CompleteAsync(cancellationToken);
            }

            return new WalletBalanceDto(
                wallet.BalanceMinutes,
                wallet.TotalEarnedMinutes,
                wallet.TotalSpentMinutes);
        }
    }
}
