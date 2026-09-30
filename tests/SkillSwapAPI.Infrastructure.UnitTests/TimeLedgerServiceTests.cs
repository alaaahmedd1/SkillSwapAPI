using FluentAssertions;
using Microsoft.EntityFrameworkCore.Storage;
using NSubstitute;
using SkillSwapAPI.Application.Common.Interfaces.UnitOfWork;
using SkillSwapAPI.Domain.Modules.Wallet.Entities;
using SkillSwapAPI.Domain.Modules.Wallet.Enums;
using SkillSwapAPI.Infrastructure.Services;
using Xunit;

namespace SkillSwapAPI.Infrastructure.UnitTests;

public sealed class TimeLedgerServiceTests
{
    [Fact]
    public async Task SettleAsync_WithValidDuration_UpdatesBothWalletsAndWritesPairedTransactions()
    {
        var learnerId = Guid.NewGuid();
        var teacherId = Guid.NewGuid();
        var swapId = Guid.NewGuid();
        var learner = Wallet(learnerId, 90);
        var teacher = Wallet(teacherId, 20);
        var fixture = CreateFixture(learner, teacher);

        await new TimeLedgerService(fixture.UnitOfWork).SettleAsync(learnerId, teacherId, 30, swapId);

        learner.BalanceMinutes.Should().Be(60);
        learner.TotalSpentMinutes.Should().Be(30);
        teacher.BalanceMinutes.Should().Be(50);
        teacher.TotalEarnedMinutes.Should().Be(50);
        await fixture.Transactions.Received(2).AddAsync(Arg.Is<TimeLedgerTransaction>(t =>
            t.SwapRequestId == swapId && t.AmountMinutes == 30));
        await fixture.UnitOfWork.Received(1).CompleteAsync(Arg.Any<CancellationToken>());
        await fixture.Transaction.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task SettleAsync_WithNonPositiveDuration_ThrowsAndDoesNotStartTransaction(int minutes)
    {
        var fixture = CreateFixture(Wallet(Guid.NewGuid(), 10), Wallet(Guid.NewGuid(), 0));

        var act = () => new TimeLedgerService(fixture.UnitOfWork)
            .SettleAsync(Guid.NewGuid(), Guid.NewGuid(), minutes, null);

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
        await fixture.UnitOfWork.DidNotReceive().BeginTransactionAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SettleAsync_WhenLearnerAndTeacherAreSame_Throws()
    {
        var userId = Guid.NewGuid();
        var fixture = CreateFixture(Wallet(userId, 10), Wallet(userId, 10));

        var act = () => new TimeLedgerService(fixture.UnitOfWork).SettleAsync(userId, userId, 1, null);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Learner and teacher must be different users.");
    }

    [Fact]
    public async Task SettleAsync_WithInsufficientBalance_RollsBackWithoutChangingWallets()
    {
        var learner = Wallet(Guid.NewGuid(), 5);
        var teacher = Wallet(Guid.NewGuid(), 10);
        var fixture = CreateFixture(learner, teacher);

        var act = () => new TimeLedgerService(fixture.UnitOfWork).SettleAsync(learner.UserId, teacher.UserId, 6, null);

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Insufficient wallet balance.");
        learner.BalanceMinutes.Should().Be(5);
        teacher.BalanceMinutes.Should().Be(10);
        await fixture.Transaction.Received(1).RollbackAsync(Arg.Any<CancellationToken>());
        await fixture.UnitOfWork.DidNotReceive().CompleteAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SettleAsync_WhenSwapWasAlreadySettled_RollsBackAndRejectsDuplicate()
    {
        var learner = Wallet(Guid.NewGuid(), 100);
        var teacher = Wallet(Guid.NewGuid(), 0);
        var fixture = CreateFixture(learner, teacher);
        fixture.Transactions.ExistsForSwapRequestAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(true);

        var act = () => new TimeLedgerService(fixture.UnitOfWork).SettleAsync(learner.UserId, teacher.UserId, 10, Guid.NewGuid());

        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("This swap request has already been settled.");
        await fixture.Transaction.Received(1).RollbackAsync(Arg.Any<CancellationToken>());
        await fixture.UnitOfWork.DidNotReceive().CompleteAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ValidateSettlementAsync_WhenEitherWalletDoesNotExist_ThrowsNotFound()
    {
        var fixture = CreateFixture(null, Wallet(Guid.NewGuid(), 10));

        var act = () => new TimeLedgerService(fixture.UnitOfWork)
            .ValidateSettlementAsync(Guid.NewGuid(), Guid.NewGuid(), 5);

        await act.Should().ThrowAsync<KeyNotFoundException>().WithMessage("Learner wallet was not found.");
    }

    private static TimeWallet Wallet(Guid userId, int balance) => new()
    {
        Id = Guid.NewGuid(), UserId = userId, BalanceMinutes = balance,
        TotalEarnedMinutes = balance, TotalSpentMinutes = 0,
        CreatedAtUtc = DateTimeOffset.UtcNow
    };

    private static (IUnitOfWork UnitOfWork, SkillSwapAPI.Application.Common.Interfaces.Repos.ITimeLedgerTransactionRepository Transactions,
        IDbContextTransaction Transaction) CreateFixture(TimeWallet? learner, TimeWallet? teacher)
    {
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var wallets = Substitute.For<SkillSwapAPI.Application.Common.Interfaces.Repos.ITimeWalletRepository>();
        var transactions = Substitute.For<SkillSwapAPI.Application.Common.Interfaces.Repos.ITimeLedgerTransactionRepository>();
        var dbTransaction = Substitute.For<IDbContextTransaction>();
        if (learner is not null) wallets.GetByUserIdAsync(learner.UserId, Arg.Any<CancellationToken>()).Returns(learner);
        if (teacher is not null) wallets.GetByUserIdAsync(teacher.UserId, Arg.Any<CancellationToken>()).Returns(teacher);
        unitOfWork.TimeWallets.Returns(wallets);
        unitOfWork.TimeLedgerTransactions.Returns(transactions);
        unitOfWork.BeginTransactionAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(dbTransaction));
        unitOfWork.CompleteAsync(Arg.Any<CancellationToken>()).Returns(1);
        return (unitOfWork, transactions, dbTransaction);
    }
}
