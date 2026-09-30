using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SkillSwapAPI.Domain.Modules.Wallet.Entities;
using SkillSwapAPI.Infrastructure.Identity;
using SkillSwapAPI.Infrastructure.Persistence.Data.DbContext;
using SkillSwapAPI.Infrastructure.Repositories;
using SkillSwapAPI.Infrastructure.Services;
using SkillSwapAPI.Infrastructure.UnitOfWork;
using Xunit;

namespace SkillSwapAPI.Infrastructure.UnitTests;

public sealed class TimeLedgerPersistenceTests
{
    [Fact]
    public async Task SettleAsync_PersistsBothWalletBalancesAndLedgerEntriesTogether()
    {
        await using var connection = await OpenDatabaseAsync();
        var (learnerId, teacherId, learnerWallet, teacherWallet) = await SeedWalletsAsync(connection, 75, 15);
        await using (var context = CreateContext(connection))
        {
            await new TimeLedgerService(new SkillSwapAPI.Infrastructure.UnitOfWork.UnitOfWork(context)).SettleAsync(learnerId, teacherId, 25, null);
        }

        await using var verification = CreateContext(connection);
        var persistedLearner = await verification.TimeWallets.SingleAsync(wallet => wallet.UserId == learnerId);
        var persistedTeacher = await verification.TimeWallets.SingleAsync(wallet => wallet.UserId == teacherId);
        var entries = await verification.TimeLedgerTransactions.OrderBy(entry => entry.TransactionType).ToListAsync();

        Assert.Equal(50, persistedLearner.BalanceMinutes);
        Assert.Equal(25, persistedLearner.TotalSpentMinutes);
        Assert.Equal(40, persistedTeacher.BalanceMinutes);
        Assert.Equal(40, persistedTeacher.TotalEarnedMinutes);
        Assert.Equal(2, entries.Count);
        Assert.Contains(entries, entry => entry.WalletId == learnerWallet && entry.TransactionType == SkillSwapAPI.Domain.Modules.Wallet.Enums.TransactionType.Spent && entry.RunningBalanceMinutes == 50);
        Assert.Contains(entries, entry => entry.WalletId == teacherWallet && entry.TransactionType == SkillSwapAPI.Domain.Modules.Wallet.Enums.TransactionType.Earned && entry.RunningBalanceMinutes == 40);
    }

    [Fact]
    public async Task SettleAsync_WhenLedgerInsertViolatesSwapForeignKey_RollsBackWalletChanges()
    {
        await using var connection = await OpenDatabaseAsync();
        var (learnerId, teacherId, _, _) = await SeedWalletsAsync(connection, 75, 15);
        await using (var context = CreateContext(connection))
        {
            var service = new TimeLedgerService(new SkillSwapAPI.Infrastructure.UnitOfWork.UnitOfWork(context));
            await Assert.ThrowsAsync<DbUpdateException>(() => service.SettleAsync(learnerId, teacherId, 25, Guid.NewGuid()));
        }

        await using var verification = CreateContext(connection);
        Assert.Equal(75, (await verification.TimeWallets.SingleAsync(wallet => wallet.UserId == learnerId)).BalanceMinutes);
        Assert.Equal(15, (await verification.TimeWallets.SingleAsync(wallet => wallet.UserId == teacherId)).BalanceMinutes);
        Assert.Empty(await verification.TimeLedgerTransactions.ToListAsync());
    }

    [Fact]
    public async Task TransactionRepository_ReturnsHistoryNewestFirst()
    {
        await using var connection = await OpenDatabaseAsync();
        var (userId, _, walletId, _) = await SeedWalletsAsync(connection, 60, 0);
        var older = Transaction(walletId, "older-ref", DateTimeOffset.UtcNow.AddHours(-1));
        var newer = Transaction(walletId, "newer-ref", DateTimeOffset.UtcNow);
        await using (var context = CreateContext(connection))
        {
            context.TimeLedgerTransactions.AddRange(older, newer);
            await context.SaveChangesAsync();
            var history = await new TimeLedgerTransactionRepository(context).GetByWalletIdAsync(walletId);
            Assert.Equal(newer.Id, history[0].Id);
            Assert.Equal(older.Id, history[1].Id);
        }
    }

    [Fact]
    public async Task WalletPersistence_RejectsSecondWalletForSameUser()
    {
        await using var connection = await OpenDatabaseAsync();
        var (userId, _, _, _) = await SeedWalletsAsync(connection, 60, 0);
        await using var context = CreateContext(connection);
        context.TimeWallets.Add(Wallet(userId, 10));

        await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
    }

    private static async Task<SqliteConnection> OpenDatabaseAsync()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync();
        return connection;
    }

    private static ApplicationDbContext CreateContext(SqliteConnection connection) =>
        new SqliteApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options);

    private static async Task<(Guid LearnerId, Guid TeacherId, Guid LearnerWalletId, Guid TeacherWalletId)> SeedWalletsAsync(
        SqliteConnection connection, int learnerBalance, int teacherBalance)
    {
        var learnerId = Guid.NewGuid();
        var teacherId = Guid.NewGuid();
        var learnerWallet = Wallet(learnerId, learnerBalance);
        var teacherWallet = Wallet(teacherId, teacherBalance);
        await using var context = CreateContext(connection);
        context.Users.AddRange(User(learnerId), User(teacherId));
        context.TimeWallets.AddRange(learnerWallet, teacherWallet);
        await context.SaveChangesAsync();
        return (learnerId, teacherId, learnerWallet.Id, teacherWallet.Id);
    }

    private static TimeWallet Wallet(Guid userId, int balance) => new()
    {
        Id = Guid.NewGuid(), UserId = userId, BalanceMinutes = balance,
        TotalEarnedMinutes = balance, TotalSpentMinutes = 0, CreatedAtUtc = DateTimeOffset.UtcNow
    };

    private static TimeLedgerTransaction Transaction(Guid walletId, string reference, DateTimeOffset createdAt) => new()
    {
        Id = Guid.NewGuid(), WalletId = walletId,
        TransactionType = SkillSwapAPI.Domain.Modules.Wallet.Enums.TransactionType.Earned,
        AmountMinutes = 10, RunningBalanceMinutes = 10,
        ReferenceCode = reference, Title = "Test entry", CreatedAtUtc = createdAt
    };

    private static AppUser User(Guid userId) => new()
    {
        Id = userId, UserName = $"user-{userId:N}", NormalizedUserName = $"USER-{userId:N}",
        Email = $"{userId:N}@example.com", NormalizedEmail = $"{userId:N}@EXAMPLE.COM",
        FirstName = "Test", LastName = "User", CreatedAtUtc = DateTimeOffset.UtcNow
    };

    private sealed class SqliteApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : ApplicationDbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // SQL Server generates rowversion values itself. SQLite has no equivalent, so
            // this test provider stores the initialized value while retaining relational transactions.
            builder.Entity<TimeWallet>().Property(wallet => wallet.RowVersion).ValueGeneratedNever();
            // SQLite cannot ORDER BY DateTimeOffset; persist its UTC instant as epoch milliseconds
            // in this test model so the repository's SQL ordering can still be exercised.
            builder.Entity<TimeLedgerTransaction>().Property(entry => entry.CreatedAtUtc)
                .HasConversion(value => value.ToUnixTimeMilliseconds(), value => DateTimeOffset.FromUnixTimeMilliseconds(value));
        }
    }
}
