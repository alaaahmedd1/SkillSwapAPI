using MediatR;
using Microsoft.Extensions.Configuration;
using SkillSwapAPI.Application.Common.Interfaces.Payments;
using SkillSwapAPI.Application.Common.Interfaces.UnitOfWork;
using SkillSwapAPI.Domain.Modules.Payments.Enums;
using SkillSwapAPI.Domain.Modules.Wallet.Entities;
using SkillSwapAPI.Domain.Modules.Wallet.Enums;

namespace SkillSwapAPI.Application.Features.Payments.Commands.ProcessPaymentWebhook;

public sealed class ProcessPaymentWebhookCommandHandler(
    IUnitOfWork unitOfWork,
    IPaymentGatewayService paymentGatewayService,
    IConfiguration configuration)
    : IRequestHandler<ProcessPaymentWebhookCommand>
{
    public async Task Handle(ProcessPaymentWebhookCommand command, CancellationToken ct)
    {
        var webhookSecret = configuration["Stripe:WebhookSecret"] ?? string.Empty;

        var webhookEvent = paymentGatewayService.ProcessWebhookEvent(
            command.JsonPayload,
            command.StripeSignature,
            webhookSecret);

        if (string.IsNullOrEmpty(webhookEvent.PaymentIntentId))
            return;

        var order = await unitOfWork.PaymentOrders.GetByPaymentIntentIdAsync(webhookEvent.PaymentIntentId, ct);
        if (order is null || order.Status == PaymentStatus.Succeeded)
            return; 

        if (webhookEvent.IsSuccess)
        {
            var package = await unitOfWork.CreditPackages.GetByIdAsync(order.CreditPackageId);
            var wallet = await unitOfWork.TimeWallets.GetByUserIdAsync(order.UserId, ct);

            if (package is not null && wallet is not null)
            {
                order.Status = PaymentStatus.Succeeded;

                wallet.BalanceMinutes += package.CreditsCount;
                wallet.TotalEarnedMinutes += package.CreditsCount;

                var transaction = new TimeLedgerTransaction
                {
                    Id = Guid.NewGuid(),
                    WalletId = wallet.Id,
                    TransactionType = TransactionType.Purchased,
                    AmountMinutes = package.CreditsCount,
                    RunningBalanceMinutes = wallet.BalanceMinutes,
                    Title = $"Purchased {package.Name} bundle",
                    ReferenceCode = order.Id.ToString(),
                    CreatedAtUtc = DateTimeOffset.UtcNow
                };

                await unitOfWork.TimeLedgerTransactions.AddAsync(transaction, ct);
            }
        }
        else
        {
            order.Status = PaymentStatus.Failed;
        }

        await unitOfWork.CompleteAsync(ct);
    }
}