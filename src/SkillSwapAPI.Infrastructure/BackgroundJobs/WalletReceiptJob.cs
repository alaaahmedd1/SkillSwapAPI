using Microsoft.AspNetCore.Identity;
using SkillSwapAPI.Application.Common.Interfaces.Notifications;
using SkillSwapAPI.Application.Common.Interfaces.Services;
using SkillSwapAPI.Application.Common.Interfaces.UnitOfWork;
using SkillSwapAPI.Infrastructure.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkillSwapAPI.Infrastructure.BackgroundJobs
{

    public sealed class WalletReceiptJob(
        IUnitOfWork unitOfWork,
        IPdfService pdfService,
        IEmailService emailService,
        IEmailTempService templateService,
        UserManager<AppUser> userManager) : IWalletReceiptJob
    {
        public async Task SendReceiptAsync(
            Guid userId,
            Guid transactionId,
            CancellationToken cancellationToken = default)
        {
            var user = await userManager.FindByIdAsync(userId.ToString());

            if (user is null || string.IsNullOrWhiteSpace(user.Email))
                return;

            var wallet = await unitOfWork.TimeWallets
                .GetByUserIdAsync(userId, cancellationToken);

            if (wallet is null)
                return;

            var transaction = await unitOfWork.TimeLedgerTransactions
                .GetByIdAsync(transactionId);

            if (transaction is null || transaction.WalletId != wallet.Id)
                return;

            var pdf = pdfService.GenerateTimeReceipt(
                transaction.Id,
                transaction.ReferenceCode,
                transaction.Title,
                transaction.TransactionType.ToString(),
                transaction.AmountMinutes,
                transaction.RunningBalanceMinutes,
                transaction.CreatedAtUtc);

            var userName = $"{user.FirstName} {user.LastName}".Trim();

            var htmlBody = templateService.GetReceiptTemplate(
                userName,
                transaction.ReferenceCode,
                transaction.CreatedAtUtc.ToString("yyyy-MM-dd HH:mm"));

            await emailService.SendWithAttachmentAsync(
                to: user.Email,
                subject: "SkillSwapAPI — Wallet Transaction Receipt",
                htmlBody: htmlBody,
                attachment: pdf,
                attachmentFileName: $"wallet-transaction-{transaction.Id}.pdf",
                contentType: "application/pdf",
                ct: cancellationToken);
        }
    }
    }
