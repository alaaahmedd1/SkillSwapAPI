using Microsoft.Extensions.Configuration;
using NSubstitute;
using SkillSwapAPI.Application.Common.Interfaces.Payments;
using SkillSwapAPI.Application.Features.Payments.Commands.InitiateCheckout;
using SkillSwapAPI.Application.Features.Payments.Commands.ProcessPaymentWebhook;
using SkillSwapAPI.Application.UnitTests.Common;
using SkillSwapAPI.Domain.Modules.Payments.Entities;
using SkillSwapAPI.Domain.Modules.Payments.Enums;
using SkillSwapAPI.Domain.Modules.Wallet.Enums;
using Xunit;

namespace SkillSwapAPI.Application.UnitTests.Features.Payments;

public sealed class PaymentWorkflowTests
{
    [Fact]
    public async Task InitiateCheckout_WithActivePackage_CreatesPendingOrderAndReturnsClientSecret()
    {
        var fixture = new UnitOfWorkFixture();
        var gateway = Substitute.For<IPaymentGatewayService>();
        var userId = Guid.NewGuid();
        var package = new CreditPackage { Id = Guid.NewGuid(), Name = "Starter", CreditsCount = 90, Price = 12.5m, Currency = "USD", IsActive = true };
        fixture.CreditPackages.GetByIdAsync(package.Id).Returns(package);
        gateway.CreatePaymentIntentAsync(package.Price, package.Currency, userId, package.Id, Arg.Any<CancellationToken>())
            .Returns(("pi-123", "secret-456"));
        PaymentOrder? storedOrder = null;
        fixture.PaymentOrders.AddAsync(Arg.Do<PaymentOrder>(order => storedOrder = order), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var response = await new InitiateCheckoutCommandHandler(fixture.UnitOfWork, gateway)
            .Handle(new InitiateCheckoutCommand(package.Id, userId), CancellationToken.None);

        Assert.NotNull(storedOrder);
        Assert.Equal(userId, storedOrder.UserId);
        Assert.Equal(package.Id, storedOrder.CreditPackageId);
        Assert.Equal(PaymentStatus.Pending, storedOrder.Status);
        Assert.Equal("pi-123", storedOrder.ExternalPaymentIntentId);
        Assert.Equal("secret-456", response.ClientSecret);
        await fixture.UnitOfWork.Received(1).CompleteAsync(Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task InitiateCheckout_WhenPackageIsMissingOrInactive_ThrowsWithoutCallingGateway(bool exists)
    {
        var fixture = new UnitOfWorkFixture();
        var gateway = Substitute.For<IPaymentGatewayService>();
        var packageId = Guid.NewGuid();
        if (exists) fixture.CreditPackages.GetByIdAsync(packageId).Returns(new CreditPackage { Id = packageId, IsActive = false });

        var act = () => new InitiateCheckoutCommandHandler(fixture.UnitOfWork, gateway)
            .Handle(new InitiateCheckoutCommand(packageId, Guid.NewGuid()), CancellationToken.None);

        await Assert.ThrowsAsync<KeyNotFoundException>(act);
        await gateway.DidNotReceive().CreatePaymentIntentAsync(Arg.Any<decimal>(), Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await fixture.UnitOfWork.DidNotReceive().CompleteAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SuccessfulWebhook_CreditsWalletAndWritesPurchaseLedgerEntry()
    {
        var fixture = new UnitOfWorkFixture();
        var gateway = Substitute.For<IPaymentGatewayService>();
        var userId = Guid.NewGuid();
        var package = new CreditPackage { Id = Guid.NewGuid(), Name = "Plus", CreditsCount = 120 };
        var wallet = TestData.TimeWallet(userId: userId, balanceMinutes: 15, totalEarned: 40, totalSpent: 25);
        var order = new PaymentOrder { Id = Guid.NewGuid(), UserId = userId, CreditPackageId = package.Id, Status = PaymentStatus.Pending, ExternalPaymentIntentId = "pi-success" };
        fixture.PaymentOrders.GetByPaymentIntentIdAsync("pi-success", Arg.Any<CancellationToken>()).Returns(order);
        fixture.CreditPackages.GetByIdAsync(package.Id).Returns(package);
        fixture.TimeWallets.GetByUserIdAsync(userId, Arg.Any<CancellationToken>()).Returns(wallet);
        gateway.ProcessWebhookEvent("payload", "signature", "webhook-secret")
            .Returns(new IPaymentGatewayService.PaymentWebhookEvent("payment_intent.succeeded", "pi-success", true, null));
        var config = Substitute.For<IConfiguration>();
        config["Stripe:WebhookSecret"].Returns("webhook-secret");
        SkillSwapAPI.Domain.Modules.Wallet.Entities.TimeLedgerTransaction? ledgerEntry = null;
        fixture.TimeLedgerTransactions.AddAsync(Arg.Do<SkillSwapAPI.Domain.Modules.Wallet.Entities.TimeLedgerTransaction>(entry => ledgerEntry = entry), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        await new ProcessPaymentWebhookCommandHandler(fixture.UnitOfWork, gateway, config)
            .Handle(new ProcessPaymentWebhookCommand("payload", "signature"), CancellationToken.None);

        Assert.Equal(PaymentStatus.Succeeded, order.Status);
        Assert.Equal(135, wallet.BalanceMinutes);
        Assert.Equal(160, wallet.TotalEarnedMinutes);
        Assert.NotNull(ledgerEntry);
        Assert.Equal(TransactionType.Purchased, ledgerEntry.TransactionType);
        Assert.Equal(120, ledgerEntry.AmountMinutes);
        Assert.Equal(135, ledgerEntry.RunningBalanceMinutes);
        Assert.Equal(order.Id.ToString(), ledgerEntry.ReferenceCode);
        await fixture.UnitOfWork.Received(1).CompleteAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task FailedWebhook_MarksOrderFailedWithoutCreditingWallet()
    {
        var fixture = new UnitOfWorkFixture();
        var gateway = Substitute.For<IPaymentGatewayService>();
        var order = new PaymentOrder { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), CreditPackageId = Guid.NewGuid(), Status = PaymentStatus.Pending, ExternalPaymentIntentId = "pi-failed" };
        fixture.PaymentOrders.GetByPaymentIntentIdAsync("pi-failed", Arg.Any<CancellationToken>()).Returns(order);
        gateway.ProcessWebhookEvent(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(new IPaymentGatewayService.PaymentWebhookEvent("payment_intent.payment_failed", "pi-failed", false, "declined"));
        var config = Substitute.For<IConfiguration>();

        await new ProcessPaymentWebhookCommandHandler(fixture.UnitOfWork, gateway, config)
            .Handle(new ProcessPaymentWebhookCommand("payload", "signature"), CancellationToken.None);

        Assert.Equal(PaymentStatus.Failed, order.Status);
        await fixture.TimeLedgerTransactions.DidNotReceive().AddAsync(Arg.Any<SkillSwapAPI.Domain.Modules.Wallet.Entities.TimeLedgerTransaction>(), Arg.Any<CancellationToken>());
        await fixture.UnitOfWork.Received(1).CompleteAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SuccessfulWebhookReplay_DoesNotCreditWalletTwice()
    {
        var fixture = new UnitOfWorkFixture();
        var gateway = Substitute.For<IPaymentGatewayService>();
        var order = new PaymentOrder { Id = Guid.NewGuid(), Status = PaymentStatus.Succeeded, ExternalPaymentIntentId = "pi-replay" };
        fixture.PaymentOrders.GetByPaymentIntentIdAsync("pi-replay", Arg.Any<CancellationToken>()).Returns(order);
        gateway.ProcessWebhookEvent(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(new IPaymentGatewayService.PaymentWebhookEvent("payment_intent.succeeded", "pi-replay", true, null));

        await new ProcessPaymentWebhookCommandHandler(fixture.UnitOfWork, gateway, Substitute.For<IConfiguration>())
            .Handle(new ProcessPaymentWebhookCommand("payload", "signature"), CancellationToken.None);

        await fixture.UnitOfWork.DidNotReceive().CompleteAsync(Arg.Any<CancellationToken>());
        await fixture.TimeLedgerTransactions.DidNotReceive().AddAsync(Arg.Any<SkillSwapAPI.Domain.Modules.Wallet.Entities.TimeLedgerTransaction>(), Arg.Any<CancellationToken>());
    }
}
