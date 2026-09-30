using Hangfire;
using NSubstitute;
using SkillSwapAPI.Application.Common.Interfaces.Services;
using SkillSwapAPI.Application.Features.Wallet.Commands.RequestWalletReceiptEmail;
using SkillSwapAPI.Application.Features.Wallet.Queries.GetWalletTransactionReceipt;
using SkillSwapAPI.Application.Features.Wallet.Queries.GetWalletTransactionDetails;
using SkillSwapAPI.Application.Features.Wallet.Queries.GetWalletTransactions;
using SkillSwapAPI.Application.Features.Wallet.Queries.GetMyWallet;
using SkillSwapAPI.Application.UnitTests.Common;
using SkillSwapAPI.Domain.Modules.Wallet.Enums;
using Xunit;

namespace SkillSwapAPI.Application.UnitTests.Features.Wallet;

public sealed class WalletWorkflowTests
{
    private readonly UnitOfWorkFixture _fixture = new();

    [Fact]
    public async Task GetMyWallet_WhenNoWalletExists_CreatesAndPersistsZeroBalanceWallet()
    {
        var userId = Guid.NewGuid();
        SkillSwapAPI.Domain.Modules.Wallet.Entities.TimeWallet? created = null;
        _fixture.TimeWallets.GetByUserIdAsync(userId, Arg.Any<CancellationToken>()).Returns((SkillSwapAPI.Domain.Modules.Wallet.Entities.TimeWallet?)null);
        _fixture.TimeWallets.AddAsync(Arg.Do<SkillSwapAPI.Domain.Modules.Wallet.Entities.TimeWallet>(wallet => created = wallet), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        var result = await new GetMyWalletQueryHandler(_fixture.UnitOfWork)
            .Handle(new GetMyWalletQuery(userId), CancellationToken.None);

        Assert.Equal(0, result.BalanceMinutes);
        Assert.Equal(0, result.TotalEarnedMinutes);
        Assert.Equal(0, result.TotalSpentMinutes);
        Assert.Equal(userId, created?.UserId);
        Assert.NotEqual(Guid.Empty, created?.Id);
        await _fixture.TimeWallets.Received(1).AddAsync(Arg.Any<SkillSwapAPI.Domain.Modules.Wallet.Entities.TimeWallet>(), Arg.Any<CancellationToken>());
        await _fixture.UnitOfWork.Received(1).CompleteAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetTransactions_WhenWalletDoesNotExist_ReturnsEmptyHistory()
    {
        var result = await new GetWalletTransactionsQueryHandler(_fixture.UnitOfWork)
            .Handle(new GetWalletTransactionsQuery(Guid.NewGuid()), CancellationToken.None);

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
    }

    [Fact]
    public async Task GetTransactions_WithFilterAndPaging_ReturnsOnlyMatchingPage()
    {
        var userId = Guid.NewGuid();
        var wallet = TestData.TimeWallet(userId: userId);
        var earnedFirst = TestData.LedgerTransaction(walletId: wallet.Id, type: TransactionType.Earned, amountMinutes: 20);
        var spent = TestData.LedgerTransaction(walletId: wallet.Id, type: TransactionType.Spent, amountMinutes: 10);
        var earnedSecond = TestData.LedgerTransaction(walletId: wallet.Id, type: TransactionType.Earned, amountMinutes: 30);
        _fixture.TimeWallets.GetByUserIdAsync(userId, Arg.Any<CancellationToken>()).Returns(wallet);
        _fixture.TimeLedgerTransactions.GetByWalletIdAsync(wallet.Id, Arg.Any<CancellationToken>())
            .Returns((IReadOnlyList<SkillSwapAPI.Domain.Modules.Wallet.Entities.TimeLedgerTransaction>)[earnedFirst, spent, earnedSecond]);
        var query = new GetWalletTransactionsQuery(userId) { Filter = WalletTransactionFilter.Earned, PageNumber = 2, PageSize = 1 };

        var result = await new GetWalletTransactionsQueryHandler(_fixture.UnitOfWork).Handle(query, CancellationToken.None);

        Assert.Equal(2, result.TotalCount);
        Assert.Single(result.Items);
        Assert.Equal(earnedSecond.Id, result.Items[0].Id);
    }

    [Fact]
    public async Task GetReceipt_WhenTransactionBelongsToAnotherWallet_ReturnsNotFoundWithoutGeneratingPdf()
    {
        var userId = Guid.NewGuid();
        var wallet = TestData.TimeWallet(userId: userId);
        var transaction = TestData.LedgerTransaction();
        var pdf = Substitute.For<IPdfService>();
        _fixture.TimeWallets.GetByUserIdAsync(userId, Arg.Any<CancellationToken>()).Returns(wallet);
        _fixture.TimeLedgerTransactions.GetByIdAsync(transaction.Id).Returns(transaction);

        var result = await new GetWalletTransactionReceiptQueryHandler(_fixture.UnitOfWork, pdf)
            .Handle(new GetWalletTransactionReceiptQuery(userId, transaction.Id), CancellationToken.None);

        Assert.False(result.IsSuccess);
        pdf.DidNotReceive().GenerateTimeReceipt(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<DateTimeOffset>());
    }

    [Fact]
    public async Task GetTransactionDetails_WhenTransactionBelongsToAnotherWallet_ReturnsNotFound()
    {
        var userId = Guid.NewGuid();
        var wallet = TestData.TimeWallet(userId: userId);
        var transaction = TestData.LedgerTransaction();
        _fixture.TimeWallets.GetByUserIdAsync(userId, Arg.Any<CancellationToken>()).Returns(wallet);
        _fixture.TimeLedgerTransactions.GetByIdAsync(transaction.Id).Returns(transaction);

        var result = await new GetWalletTransactionDetailsQueryHandler(_fixture.UnitOfWork)
            .Handle(new GetWalletTransactionDetailsQuery(userId, transaction.Id), CancellationToken.None);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task GetReceipt_WhenTransactionBelongsToUser_GeneratesReceiptFromPersistedDetails()
    {
        var userId = Guid.NewGuid();
        var wallet = TestData.TimeWallet(userId: userId);
        var transaction = TestData.LedgerTransaction(walletId: wallet.Id, type: TransactionType.Spent, amountMinutes: 25, runningBalance: 75);
        var pdf = Substitute.For<IPdfService>();
        pdf.GenerateTimeReceipt(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<DateTimeOffset>()).Returns([1, 2, 3]);
        _fixture.TimeWallets.GetByUserIdAsync(userId, Arg.Any<CancellationToken>()).Returns(wallet);
        _fixture.TimeLedgerTransactions.GetByIdAsync(transaction.Id).Returns(transaction);

        var result = await new GetWalletTransactionReceiptQueryHandler(_fixture.UnitOfWork, pdf)
            .Handle(new GetWalletTransactionReceiptQuery(userId, transaction.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(new byte[] { 1, 2, 3 }, result.Value);
        pdf.Received(1).GenerateTimeReceipt(transaction.Id, transaction.ReferenceCode, transaction.Title, "Spent", 25, 75, transaction.CreatedAtUtc);
    }

    [Fact]
    public async Task RequestReceiptEmail_WhenTransactionBelongsToUser_QueuesJobWithOwnerAndTransaction()
    {
        var userId = Guid.NewGuid();
        var wallet = TestData.TimeWallet(userId: userId);
        var transaction = TestData.LedgerTransaction(walletId: wallet.Id);
        var jobs = Substitute.For<IBackgroundJobClient>();
        jobs.Create(Arg.Any<Hangfire.Common.Job>(), Arg.Any<Hangfire.States.IState>()).Returns("queued-job");
        _fixture.TimeWallets.GetByUserIdAsync(userId, Arg.Any<CancellationToken>()).Returns(wallet);
        _fixture.TimeLedgerTransactions.GetByIdAsync(transaction.Id).Returns(transaction);

        var result = await new RequestWalletReceiptEmailCommandHandler(_fixture.UnitOfWork, jobs)
            .Handle(new RequestWalletReceiptEmailCommand(userId, transaction.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        jobs.Received(1).Create(Arg.Is<Hangfire.Common.Job>(job =>
            job.Args.Any(argument => Equals(argument, userId)) &&
            job.Args.Any(argument => Equals(argument, transaction.Id))), Arg.Any<Hangfire.States.IState>());
    }

    [Fact]
    public async Task RequestReceiptEmail_WhenTransactionBelongsToAnotherWallet_DoesNotQueueJob()
    {
        var userId = Guid.NewGuid();
        var wallet = TestData.TimeWallet(userId: userId);
        var transaction = TestData.LedgerTransaction();
        var jobs = Substitute.For<IBackgroundJobClient>();
        _fixture.TimeWallets.GetByUserIdAsync(userId, Arg.Any<CancellationToken>()).Returns(wallet);
        _fixture.TimeLedgerTransactions.GetByIdAsync(transaction.Id).Returns(transaction);

        var result = await new RequestWalletReceiptEmailCommandHandler(_fixture.UnitOfWork, jobs)
            .Handle(new RequestWalletReceiptEmailCommand(userId, transaction.Id), CancellationToken.None);

        Assert.False(result.IsSuccess);
        jobs.DidNotReceive().Create(Arg.Any<Hangfire.Common.Job>(), Arg.Any<Hangfire.States.IState>());
    }
}
