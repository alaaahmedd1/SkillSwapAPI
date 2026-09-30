using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SkillSwapAPI.Domain.Modules.Payments.Entities;
using SkillSwapAPI.Domain.Modules.SessionProposals.Entities;
using SkillSwapAPI.Domain.Modules.SessionProposals.Enums;
using SkillSwapAPI.Domain.Modules.SwapRequests.Entities;
using SkillSwapAPI.Domain.Modules.SwapRequests.Enums;
using SkillSwapAPI.Domain.Modules.Wallet.Entities;
using SkillSwapAPI.Domain.Modules.Wallet.Enums;
using SkillSwapAPI.Infrastructure.Persistence.Data.DbContext;

namespace SkillSwapAPI.LoadTests;

public sealed class DatabaseSeeder(string connectionString)
{
    private ApplicationDbContext CreateContext() =>
        new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(connectionString)
            .Options);

    private SqlConnection OpenConnection()
    {
        var connection = new SqlConnection(connectionString);
        connection.Open();
        return connection;
    }

    public void EnsureDatabase()
    {
        var builder = new SqlConnectionStringBuilder(connectionString);
        var databaseName = builder.InitialCatalog;
        builder.InitialCatalog = "master";

        using var connection = new SqlConnection(builder.ConnectionString);
        connection.Open();

        using var check = new SqlCommand(
            "SELECT COUNT(*) FROM sys.databases WHERE name = @name", connection);
        check.Parameters.AddWithValue("@name", databaseName);

        if ((int)check.ExecuteScalar()! > 0)
        {
            return;
        }

        using var create = new SqlCommand($"CREATE DATABASE [{databaseName}]", connection);
        create.ExecuteNonQuery();
        Console.WriteLine($"[seed] Database {databaseName} created.");
    }

    public void Migrate()
    {
        using var context = CreateContext();
        context.Database.Migrate();
        Console.WriteLine("[seed] Migrations applied.");
    }

    public void ConfirmEmails(string emailPattern)
    {
        using var connection = OpenConnection();
        using var command = new SqlCommand(
            "UPDATE AspNetUsers SET EmailConfirmed = 1 WHERE Email LIKE @pattern", connection);
        command.Parameters.AddWithValue("@pattern", emailPattern);
        Console.WriteLine($"[seed] EmailConfirmed set for {command.ExecuteNonQuery()} users.");
    }

    public void GrantAdminRole(Guid userId)
    {
        using var connection = OpenConnection();
        using var command = new SqlCommand(
            """
            IF NOT EXISTS (SELECT 1 FROM AspNetUserRoles WHERE UserId = @userId)
            BEGIN
                INSERT INTO AspNetUserRoles (UserId, RoleId)
                SELECT @userId, Id FROM AspNetRoles WHERE Name = 'Admin'
            END
            """, connection);
        command.Parameters.AddWithValue("@userId", userId);
        command.ExecuteNonQuery();
        Console.WriteLine("[seed] Admin role granted.");
    }

    public Guid? GetUserId(string email)
    {
        using var connection = OpenConnection();
        using var command = new SqlCommand(
            "SELECT Id FROM AspNetUsers WHERE Email = @email", connection);
        command.Parameters.AddWithValue("@email", email);
        return command.ExecuteScalar() as Guid?;
    }

    public int CountUsers(string emailPattern)
    {
        using var connection = OpenConnection();
        using var command = new SqlCommand(
            "SELECT COUNT(*) FROM AspNetUsers WHERE Email LIKE @pattern", connection);
        command.Parameters.AddWithValue("@pattern", emailPattern);
        return (int)command.ExecuteScalar()!;
    }

    public int CountRows(string table)
    {
        using var connection = OpenConnection();
        using var command = new SqlCommand($"SELECT COUNT(*) FROM [{table}]", connection);
        return (int)command.ExecuteScalar()!;
    }

    public Dictionary<string, string> GetOtpTokens(string loginProvider, string name, string emailPattern)
    {
        var tokens = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        using var connection = OpenConnection();
        using var command = new SqlCommand(
            """
            SELECT u.Email, t.Value
            FROM AspNetUserTokens t
            INNER JOIN AspNetUsers u ON u.Id = t.UserId
            WHERE t.LoginProvider = @provider AND t.Name = @name AND u.Email LIKE @pattern
            """, connection);
        command.Parameters.AddWithValue("@provider", loginProvider);
        command.Parameters.AddWithValue("@name", name);
        command.Parameters.AddWithValue("@pattern", emailPattern);

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            tokens[reader.GetString(0)] = reader.GetString(1);
        }

        Console.WriteLine($"[seed] Read {tokens.Count} '{loginProvider}/{name}' tokens.");
        return tokens;
    }

    public Guid? FindConversationId(Guid swapRequestId)
    {
        using var context = CreateContext();
        return context.Conversations
            .AsNoTracking()
            .Where(item => item.SwapRequestId == swapRequestId)
            .Select(item => (Guid?)item.Id)
            .FirstOrDefault();
    }

    public Guid? FindLiveSessionRoomId(Guid swapRequestId)
    {
        using var context = CreateContext();
        return context.LiveSessionRooms
            .AsNoTracking()
            .Where(item => item.SwapRequestId == swapRequestId)
            .Select(item => (Guid?)item.Id)
            .FirstOrDefault();
    }

    public Guid SeedCreditPackages()
    {
        using var context = CreateContext();

        var existing = context.CreditPackages.AsNoTracking().FirstOrDefault();
        if (existing is not null)
        {
            Console.WriteLine("[seed] Credit packages already present.");
            return existing.Id;
        }

        var starter = new CreditPackage
        {
            Id = Guid.NewGuid(),
            Name = "Starter",
            Description = "120 minutes of skill exchange time",
            CreditsCount = 120,
            Price = 9.99m,
            Currency = "USD",
            IsActive = true
        };

        context.CreditPackages.AddRange(
            starter,
            new CreditPackage
            {
                Id = Guid.NewGuid(),
                Name = "Standard",
                Description = "300 minutes of skill exchange time",
                CreditsCount = 300,
                Price = 19.99m
            },
            new CreditPackage
            {
                Id = Guid.NewGuid(),
                Name = "Pro",
                Description = "600 minutes of skill exchange time",
                CreditsCount = 600,
                Price = 34.99m
            });

        context.SaveChanges();
        Console.WriteLine("[seed] Credit packages inserted.");
        return starter.Id;
    }

    public Guid SeedWallet(Guid userId, int balanceMinutes)
    {
        using var context = CreateContext();

        var wallet = context.TimeWallets.FirstOrDefault(item => item.UserId == userId);
        if (wallet is null)
        {
            wallet = new TimeWallet
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                BalanceMinutes = balanceMinutes,
                TotalEarnedMinutes = balanceMinutes,
                TotalSpentMinutes = 0,
                CreatedAtUtc = DateTimeOffset.UtcNow
            };
            context.TimeWallets.Add(wallet);
            context.SaveChanges();
        }
        else if (wallet.BalanceMinutes < balanceMinutes)
        {
            wallet.BalanceMinutes = balanceMinutes;
            wallet.TotalEarnedMinutes = balanceMinutes;
            wallet.UpdatedAtUtc = DateTimeOffset.UtcNow;
            context.SaveChanges();
        }

        return wallet.Id;
    }

    public List<Guid> SeedWalletTransactions(Guid walletId, Guid partnerUserId, int count)
    {
        using var context = CreateContext();

        var existing = context.TimeLedgerTransactions
            .AsNoTracking()
            .Where(item => item.WalletId == walletId)
            .Select(item => item.Id)
            .ToList();

        if (existing.Count >= count)
        {
            Console.WriteLine($"[seed] {existing.Count} wallet transactions already present.");
            return existing;
        }

        var running = 600;
        var added = new List<Guid>(count);
        for (var i = 0; i < count; i++)
        {
            var id = Guid.NewGuid();
            running -= 5;
            added.Add(id);
            context.TimeLedgerTransactions.Add(new TimeLedgerTransaction
            {
                Id = id,
                WalletId = walletId,
                TransactionType = TransactionType.Spent,
                AmountMinutes = 5,
                RunningBalanceMinutes = running,
                ReferenceCode = $"LT-{id:N}"[..24],
                Title = "Session settlement",
                PartnerUserId = partnerUserId,
                CreatedAtUtc = DateTimeOffset.UtcNow.AddMinutes(-i)
            });
        }

        context.SaveChanges();
        Console.WriteLine($"[seed] {added.Count} wallet transactions inserted.");
        return added;
    }

    public List<Guid> GetWalletTransactionIds(Guid userId, int count)
    {
        using var context = CreateContext();
        return context.TimeLedgerTransactions
            .AsNoTracking()
            .Join(
                context.TimeWallets.AsNoTracking().Where(wallet => wallet.UserId == userId),
                transaction => transaction.WalletId,
                wallet => wallet.Id,
                (transaction, _) => transaction.Id)
            .Take(count)
            .ToList();
    }

    public List<Guid> SeedSwaps(
        string tag,
        Guid requesterId,
        List<Guid> receiverIds,
        Guid offeredSkillId,
        Guid requestedSkillId,
        SwapRequestStatus status,
        bool requesterConfirmed,
        bool receiverConfirmed,
        int count)
    {
        using var context = CreateContext();

        var existing = context.SwapRequests
            .AsNoTracking()
            .Where(item => item.ProposedScheduleDetails == tag)
            .Select(item => item.Id)
            .Take(count)
            .ToList();

        if (existing.Count >= count)
        {
            Console.WriteLine($"[seed] {existing.Count} swaps for '{tag}' already present.");
            return existing;
        }

        var ids = new List<Guid>(count);
        for (var i = 0; i < count; i++)
        {
            var id = Guid.NewGuid();
            ids.Add(id);
            var receiverId = receiverIds.Count == 0 ? Guid.Empty : receiverIds[i % receiverIds.Count];
            context.SwapRequests.Add(new SwapRequest
            {
                Id = id,
                RequesterId = requesterId,
                ReceiverId = receiverId,
                OfferedSkillId = offeredSkillId,
                RequestedSkillId = requestedSkillId,
                Status = status,
                IsRequesterConfirmed = requesterConfirmed,
                IsReceiverConfirmed = receiverConfirmed,
                ProposedScheduleDetails = tag,
                CreatedAtUtc = DateTimeOffset.UtcNow.AddDays(-1),
                UpdatedAtUtc = DateTimeOffset.UtcNow
            });
        }

        context.SaveChanges();
        Console.WriteLine($"[seed] {ids.Count} swaps inserted for '{tag}' ({status}).");
        return ids;
    }

    public List<Guid> SeedProposals(List<Guid> swapRequestIds, Guid proposerId)
    {
        using var context = CreateContext();

        var existingBySwap = context.SessionProposals
            .AsNoTracking()
            .Where(item => swapRequestIds.Contains(item.SwapRequestId))
            .ToDictionary(item => item.SwapRequestId, item => item.Id);

        var result = new List<Guid>(swapRequestIds.Count);
        var scheduledDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);
        var startTime = new TimeOnly(18, 0);
        var added = 0;

        foreach (var swapId in swapRequestIds)
        {
            if (existingBySwap.TryGetValue(swapId, out var existingId))
            {
                result.Add(existingId);
                continue;
            }

            var id = Guid.NewGuid();
            result.Add(id);
            added++;
            context.SessionProposals.Add(new SessionProposal
            {
                Id = id,
                SwapRequestId = swapId,
                ProposerId = proposerId,
                ScheduledDate = scheduledDate,
                StartTime = startTime,
                EndTime = startTime.AddMinutes(60),
                DurationMinutes = 60,
                Status = ProposalStatus.Proposed,
                CreatedAtUtc = DateTimeOffset.UtcNow
            });
        }

        if (added > 0)
        {
            context.SaveChanges();
        }

        Console.WriteLine($"[seed] {added} session proposals inserted ({result.Count} total).");
        return result;
    }

    public Dictionary<Guid, Guid> GetUserSkillIds(Guid skillId, IReadOnlyCollection<Guid> userIds)
    {
        using var context = CreateContext();
        return context.UserSkills
            .AsNoTracking()
            .Where(item => item.SkillId == skillId && userIds.Contains(item.UserId))
            .ToDictionary(item => item.UserId, item => item.Id);
    }

    public Guid? GetUserSkillId(Guid userId, Guid skillId)
    {
        using var context = CreateContext();
        return context.UserSkills
            .AsNoTracking()
            .Where(item => item.UserId == userId && item.SkillId == skillId)
            .Select(item => (Guid?)item.Id)
            .FirstOrDefault();
    }

    public List<Guid> GetReviewedSwapIds(Guid reviewerId, int count)    {
        using var context = CreateContext();

        var reviewed = context.Reviews
            .AsNoTracking()
            .Where(review => review.ReviewerId == reviewerId)
            .Select(review => review.SwapRequestId);

        return context.SwapRequests
            .AsNoTracking()
            .Where(item => item.Status == SwapRequestStatus.Completed
                           && (item.RequesterId == reviewerId || item.ReceiverId == reviewerId)
                           && !reviewed.Contains(item.Id))
            .Select(item => item.Id)
            .Take(count)
            .ToList();
    }
}
