using Microsoft.EntityFrameworkCore;
using SkillSwapAPI.Application.Common.Interfaces.Services;
using SkillSwapAPI.Application.Common.Interfaces.UnitOfWork;
using SkillSwapAPI.Domain.Modules.Wallet.Entities;
using SkillSwapAPI.Domain.Modules.Wallet.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkillSwapAPI.Infrastructure.Services
{
    public sealed class TimeLedgerService : ITimeLedgerService
    {
        private readonly IUnitOfWork _unitOfWork;

        public TimeLedgerService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task SettleAsync(
     Guid learnerId,
     Guid teacherId,
     int durationMinutes,
     Guid? swapRequestId,
     CancellationToken cancellationToken = default)
        {
            if (learnerId == teacherId)
            {
                throw new InvalidOperationException(
                    "Learner and teacher must be different users.");
            }

            if (durationMinutes <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(durationMinutes),
                    "Duration must be greater than zero.");
            }

            await using var transaction =
                await _unitOfWork.BeginTransactionAsync(cancellationToken);

            try
            {
                if (swapRequestId.HasValue)
                {
                    var alreadySettled = await _unitOfWork
                        .TimeLedgerTransactions
                        .ExistsForSwapRequestAsync(
                            swapRequestId.Value,
                            cancellationToken);

                    if (alreadySettled)
                    {
                        throw new InvalidOperationException(
                            "This swap request has already been settled.");
                    }
                }

                var learnerWallet = await _unitOfWork.TimeWallets
                    .GetByUserIdAsync(
                        learnerId,
                        cancellationToken);

                var teacherWallet = await _unitOfWork.TimeWallets
                    .GetByUserIdAsync(
                        teacherId,
                        cancellationToken);

                if (learnerWallet is null)
                {
                    throw new KeyNotFoundException(
                        "Learner wallet was not found.");
                }

                if (teacherWallet is null)
                {
                    throw new KeyNotFoundException(
                        "Teacher wallet was not found.");
                }

                if (learnerWallet.BalanceMinutes < durationMinutes)
                {
                    throw new InvalidOperationException(
                        "Insufficient wallet balance.");
                }

                var now = DateTimeOffset.UtcNow;

                learnerWallet.BalanceMinutes -= durationMinutes;
                learnerWallet.TotalSpentMinutes += durationMinutes;
                learnerWallet.UpdatedAtUtc = now;

                teacherWallet.BalanceMinutes += durationMinutes;
                teacherWallet.TotalEarnedMinutes += durationMinutes;
                teacherWallet.UpdatedAtUtc = now;

                var spentTransaction = new TimeLedgerTransaction
                {
                    Id = Guid.NewGuid(),
                    WalletId = learnerWallet.Id,
                    SwapRequestId = swapRequestId,
                    TransactionType = TransactionType.Spent,
                    AmountMinutes = durationMinutes,
                    RunningBalanceMinutes = learnerWallet.BalanceMinutes,
                    ReferenceCode = GenerateReferenceCode(),
                    Title = "Skill Exchange - Time Spent",
                    PartnerUserId = teacherId,
                    CreatedAtUtc = now
                };

                var earnedTransaction = new TimeLedgerTransaction
                {
                    Id = Guid.NewGuid(),
                    WalletId = teacherWallet.Id,
                    SwapRequestId = swapRequestId,
                    TransactionType = TransactionType.Earned,
                    AmountMinutes = durationMinutes,
                    RunningBalanceMinutes = teacherWallet.BalanceMinutes,
                    ReferenceCode = GenerateReferenceCode(),
                    Title = "Skill Exchange - Time Earned",
                    PartnerUserId = learnerId,
                    CreatedAtUtc = now
                };

                await _unitOfWork.TimeLedgerTransactions
                    .AddAsync(spentTransaction);

                await _unitOfWork.TimeLedgerTransactions
                    .AddAsync(earnedTransaction);

                await _unitOfWork.CompleteAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync(cancellationToken);

                throw new InvalidOperationException(
                    "The wallet was updated by another transaction. Please try again.");
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
        private static string GenerateReferenceCode()
        {
            return $"SWAP-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}"[..50];
        }
    }
}
