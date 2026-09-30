using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using SkillSwapAPI.Application.Common.Interfaces.Notifications;
using SkillSwapAPI.Application.Common.Interfaces.Repos;
using SkillSwapAPI.Application.Common.Interfaces.Services;
using SkillSwapAPI.Application.Common.Interfaces.UnitOfWork;
using SkillSwapAPI.Domain.Modules.Wallet.Entities;
using SkillSwapAPI.Infrastructure.BackgroundJobs;
using SkillSwapAPI.Infrastructure.Identity;
using Xunit;

namespace SkillSwapAPI.Infrastructure.UnitTests;

public sealed class WalletReceiptJobTests
{
    [Fact]
    public async Task SendReceiptAsync_ForOwnedTransaction_SendsReceiptPdfToAccountEmail()
    {
        var userId = Guid.NewGuid();
        var user = new AppUser { Id = userId, Email = "owner@example.com", FirstName = "Ada", LastName = "Lovelace" };
        var wallet = new TimeWallet { Id = Guid.NewGuid(), UserId = userId };
        var transaction = new TimeLedgerTransaction { Id = Guid.NewGuid(), WalletId = wallet.Id, ReferenceCode = "REF-77", Title = "Time earned", AmountMinutes = 30, RunningBalanceMinutes = 90, CreatedAtUtc = DateTimeOffset.UtcNow };
        var (job, unitOfWork, pdf, email, templates, userManager) = CreateJob(user, wallet, transaction);
        pdf.GenerateTimeReceipt(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<DateTimeOffset>())
            .Returns(new byte[] { 1, 2, 3, 4 });
        templates.GetReceiptTemplate("Ada Lovelace", transaction.ReferenceCode, Arg.Any<string>()).Returns("receipt html");

        await job.SendReceiptAsync(userId, transaction.Id);

        await email.Received(1).SendWithAttachmentAsync(
            "owner@example.com", "SkillSwapAPI — Wallet Transaction Receipt", "receipt html",
            Arg.Any<byte[]>(),
            $"wallet-transaction-{transaction.Id}.pdf", "application/pdf", Arg.Any<CancellationToken>());
        Assert.Equal(new byte[] { 1, 2, 3, 4 }, email.ReceivedCalls().Single().GetArguments()[3]);
        await userManager.Received(1).FindByIdAsync(userId.ToString());
    }

    [Fact]
    public async Task SendReceiptAsync_WhenTransactionBelongsToAnotherWallet_DoesNotSendPrivateReceipt()
    {
        var userId = Guid.NewGuid();
        var user = new AppUser { Id = userId, Email = "owner@example.com" };
        var wallet = new TimeWallet { Id = Guid.NewGuid(), UserId = userId };
        var transaction = new TimeLedgerTransaction { Id = Guid.NewGuid(), WalletId = Guid.NewGuid(), ReferenceCode = "REF-77", Title = "Private", AmountMinutes = 10, RunningBalanceMinutes = 10 };
        var (job, _, pdf, email, _, _) = CreateJob(user, wallet, transaction);

        await job.SendReceiptAsync(userId, transaction.Id);

        await email.DidNotReceive().SendWithAttachmentAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<byte[]>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        pdf.DidNotReceiveWithAnyArgs().GenerateTimeReceipt(default, default!, default!, default!, default, default, default);
    }

    private static (WalletReceiptJob Job, IUnitOfWork UnitOfWork, IPdfService Pdf, IEmailService Email, IEmailTempService Templates, UserManager<AppUser> UserManager)
        CreateJob(AppUser user, TimeWallet wallet, TimeLedgerTransaction transaction)
    {
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var wallets = Substitute.For<ITimeWalletRepository>();
        var transactions = Substitute.For<ITimeLedgerTransactionRepository>();
        wallets.GetByUserIdAsync(user.Id, Arg.Any<CancellationToken>()).Returns(wallet);
        transactions.GetByIdAsync(transaction.Id).Returns(transaction);
        unitOfWork.TimeWallets.Returns(wallets);
        unitOfWork.TimeLedgerTransactions.Returns(transactions);
        var store = Substitute.For<IUserStore<AppUser>>();
        var userManager = Substitute.For<UserManager<AppUser>>(
            store,
            Options.Create(new IdentityOptions()),
            Substitute.For<IPasswordHasher<AppUser>>(),
            Array.Empty<IUserValidator<AppUser>>(),
            Array.Empty<IPasswordValidator<AppUser>>(),
            Substitute.For<ILookupNormalizer>(),
            new IdentityErrorDescriber(),
            Substitute.For<IServiceProvider>(),
            Substitute.For<ILogger<UserManager<AppUser>>>());
        userManager.FindByIdAsync(user.Id.ToString()).Returns(user);
        var pdf = Substitute.For<IPdfService>();
        var email = Substitute.For<IEmailService>();
        var templates = Substitute.For<IEmailTempService>();
        return (new WalletReceiptJob(unitOfWork, pdf, email, templates, userManager), unitOfWork, pdf, email, templates, userManager);
    }
}
