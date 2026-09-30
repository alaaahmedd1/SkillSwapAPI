using SkillSwapAPI.Domain.Modules.SwapRequests.Enums;
using System.Text.Json;

namespace SkillSwapAPI.LoadTests;

public sealed class ApiSeeder(ApiClient client, DatabaseSeeder db, LoadOptions options)
{
    private const int SeedParallelism = 32;

    private static object RegisterBody(string email) => new
    {
        firstName = "Load",
        lastName = email.Split('@')[0].Replace('.', ' '),
        email,
        password = SeedState.Password
    };

    public async Task<SeedState> SeedAsync()
    {
        var state = new SeedState();

        await RegisterCoreUsersAsync(state);
        await RegisterBatchAsync(state.PoolEmailAt, options.PoolSize);
        await RegisterBatchAsync(state.OtpEmailAt, options.Concurrency);
        await RegisterBatchAsync(state.ResetEmailAt, options.Concurrency);
        await RegisterBatchAsync(state.TokenEmailAt, options.Concurrency);

        ConfirmEmails();

        state.AdminId = db.GetUserId(state.AdminEmail) ?? throw new InvalidOperationException("Admin user missing.");
        state.AliceId = db.GetUserId(state.AliceEmail) ?? throw new InvalidOperationException("Alice missing.");
        state.BobId = db.GetUserId(state.BobEmail) ?? throw new InvalidOperationException("Bob missing.");
        state.HammerId = db.GetUserId(state.HammerEmail) ?? throw new InvalidOperationException("Hammer missing.");
        db.GrantAdminRole(state.AdminId);

        state.AdminToken = await LoginAsync(state.AdminEmail);
        state.AliceToken = await LoginAsync(state.AliceEmail);
        state.BobToken = await LoginAsync(state.BobEmail);

        await LoginPoolAsync(state);
        await LoginTokenUsersAsync(state);
        await PrepareResetUsersAsync(state);
        ReadOtpCodes(state);

        await CreateCatalogAsync(state);
        await AssignSkillsAsync(state);
        await CreateLifecycleSwapsAsync(state);
        await SeedBulkDataAsync(state);

        return state;
    }

    private async Task RegisterCoreUsersAsync(SeedState state)
    {
        foreach (var email in new[] { state.AdminEmail, state.AliceEmail, state.BobEmail, state.HammerEmail })
        {
            await RegisterAsync(email);
        }
    }

    private async Task RegisterBatchAsync(Func<int, string> emailAt, int count)
    {
        if (count <= 0)
        {
            return;
        }

        await RunParallelAsync(count, async index => await RegisterAsync(emailAt(index)));
    }

    private async Task RegisterAsync(string email)
    {
        var (status, content) = await client.PostJsonAsync("/api/auth/register", RegisterBody(email));
        if (status is not (200 or 201 or 409))
        {
            Console.WriteLine($"[seed] register {email} -> {status} {Truncate(content)}");
        }
    }

    private void ConfirmEmails()
    {
        db.ConfirmEmails("load.core.%");
        db.ConfirmEmails("load.pool.%");
        db.ConfirmEmails("load.reset.%");
        db.ConfirmEmails("load.token.%");
    }

    private async Task<string> LoginAsync(string email)
    {
        var (status, content) = await client.PostJsonAsync(
            "/api/auth/login", new { email, password = SeedState.Password });

        if (status != 200)
        {
            throw new InvalidOperationException($"Login failed for {email}: {status} {Truncate(content)}");
        }

        var json = SeedState.ParseJson(content);
        return SeedState.JsonString(json, "accessToken")
               ?? throw new InvalidOperationException($"No access token for {email}: {Truncate(content)}");
    }

    private async Task LoginPoolAsync(SeedState state)
    {
        state.PoolEmails.Clear();
        state.PoolTokens.Clear();
        state.PoolUserIds.Clear();

        var size = options.PoolSize;
        var tokens = new string[size];

        await RunParallelAsync(size, async index =>
        {
            var email = state.PoolEmailAt(index);
            var (status, content) = await client.PostJsonAsync(
                "/api/auth/login", new { email, password = SeedState.Password });

            if (status != 200)
            {
                return;
            }

            tokens[index] = SeedState.JsonString(SeedState.ParseJson(content), "accessToken") ?? "";
        });

        for (var index = 0; index < size; index++)
        {
            if (string.IsNullOrEmpty(tokens[index]))
            {
                continue;
            }

            var email = state.PoolEmailAt(index);
            state.PoolEmails.Add(email);
            state.PoolTokens.Add(tokens[index]);
            state.PoolUserIds.Add(db.GetUserId(email) ?? Guid.Empty);
        }

        Console.WriteLine($"[seed] {state.PoolTokens.Count} pool users logged in.");
    }

    private async Task LoginTokenUsersAsync(SeedState state)
    {
        var size = options.Concurrency;
        var pairs = new TokenPair?[size];

        await RunParallelAsync(size, async index =>
        {
            var email = state.TokenEmailAt(index);
            var (status, content) = await client.PostJsonAsync(
                "/api/auth/login", new { email, password = SeedState.Password });

            if (status != 200)
            {
                return;
            }

            var json = SeedState.ParseJson(content);
            var access = SeedState.JsonString(json, "accessToken");
            var refresh = SeedState.JsonString(json, "refreshToken");
            if (access is not null && refresh is not null)
            {
                pairs[index] = new TokenPair(refresh, access);
            }
        });

        foreach (var pair in pairs)
        {
            if (pair is not null)
            {
                state.TokenPairs.Add(pair);
            }
        }

        Console.WriteLine($"[seed] {state.TokenPairs.Count} refresh tokens collected.");
    }

    private async Task PrepareResetUsersAsync(SeedState state)
    {
        var size = options.Concurrency;
        var emails = new string[size];
        for (var index = 0; index < size; index++)
        {
            emails[index] = state.ResetEmailAt(index);
        }

        await RunParallelAsync(size, async index =>
            await client.PostJsonAsync("/api/auth/forgot-password", new { email = emails[index] }));

        state.ResetCodes.Clear();
        foreach (var pair in db.GetOtpTokens("Email", "PasswordReset", "load.reset.%"))
        {
            state.ResetCodes[pair.Key] = pair.Value;
        }

        foreach (var email in emails)
        {
            if (state.ResetCodes.ContainsKey(email))
            {
                state.ResetEmails.Add(email);
            }
        }

        Console.WriteLine($"[seed] {state.ResetEmails.Count} password-reset users ready.");
    }

    private void ReadOtpCodes(SeedState state)
    {
        state.OtpCodes.Clear();
        foreach (var pair in db.GetOtpTokens("Email", "EmailConfirmation", "load.otp.%"))
        {
            state.OtpCodes[pair.Key] = pair.Value;
        }

        state.OtpEmails.Clear();
        for (var index = 0; index < options.Concurrency; index++)
        {
            var email = state.OtpEmailAt(index);
            if (state.OtpCodes.ContainsKey(email))
            {
                state.OtpEmails.Add(email);
            }
        }

        Console.WriteLine($"[seed] {state.OtpEmails.Count} unconfirmed OTP users ready.");
    }

    private async Task CreateCatalogAsync(SeedState state)
    {
        var stamp = state.RunStamp;

        var (categoryStatus, categoryBody) = await client.PostJsonAsync(
            "/api/v1/admin/categories",
            new { name = $"Load Category {stamp}", description = "Seeded for load testing" },
            state.AdminToken);

        if (categoryStatus is not (200 or 201))
        {
            throw new InvalidOperationException($"Category creation failed: {categoryStatus} {Truncate(categoryBody)}");
        }

        state.CategoryId = SeedState.JsonInt(SeedState.ParseJson(categoryBody), "id")
                           ?? throw new InvalidOperationException($"No category id in {Truncate(categoryBody)}");

        state.OfferedSkillId = await CreateSkillAsync(state, $"Load Offered {stamp}");
        state.RequestedSkillId = await CreateSkillAsync(state, $"Load Requested {stamp}");
        state.RemovableSkillId = await CreateSkillAsync(state, $"Load Removable {stamp}");
        state.LoadRunSkillId = await CreateSkillAsync(state, $"Load Run {stamp}");
        state.AddSkillTargetId = await CreateSkillAsync(state, $"Load Add {stamp}");
        BulkSkillA = await CreateSkillAsync(state, $"Load Bulk A {stamp}");
        BulkSkillB = await CreateSkillAsync(state, $"Load Bulk B {stamp}");
        BulkSkillC = await CreateSkillAsync(state, $"Load Bulk C {stamp}");
        BulkSkillD = await CreateSkillAsync(state, $"Load Bulk D {stamp}");
        BulkSkillE = await CreateSkillAsync(state, $"Load Bulk E {stamp}");

        Console.WriteLine($"[seed] Category {state.CategoryId} with 10 skills created.");
    }

    public Guid BulkSkillA { get; private set; }
    public Guid BulkSkillB { get; private set; }
    public Guid BulkSkillC { get; private set; }
    public Guid BulkSkillD { get; private set; }
    public Guid BulkSkillE { get; private set; }

    private async Task<Guid> CreateSkillAsync(SeedState state, string name)
    {
        var (status, body) = await client.PostJsonAsync(
            "/api/v1/admin/skills",
            new { categoryId = state.CategoryId, name, description = "Seeded for load testing" },
            state.AdminToken);

        if (status is not (200 or 201))
        {
            throw new InvalidOperationException($"Skill '{name}' creation failed: {status} {Truncate(body)}");
        }

        return SeedState.JsonGuid(SeedState.ParseJson(body), "id")
               ?? throw new InvalidOperationException($"No skill id in {Truncate(body)}");
    }

    private async Task AssignSkillsAsync(SeedState state)
    {
        await AddSkillAsync(state.BobToken, state.RequestedSkillId);

        foreach (var skillId in new[] { BulkSkillA, BulkSkillB, BulkSkillC, BulkSkillD, BulkSkillE })
        {
            await AddSkillAsync(state.BobToken, skillId);
        }

        var aliceSkill = await AddSkillAsync(state.AliceToken, state.OfferedSkillId);
        state.AliceUserSkillId = aliceSkill ?? db.GetUserSkillId(state.AliceId, state.OfferedSkillId) ?? Guid.Empty;

        await RunParallelAsync(state.PoolTokens.Count, async index =>
        {
            await AddSkillAsync(state.PoolTokens[index], state.LoadRunSkillId);
            await AddSkillAsync(state.PoolTokens[index], state.RemovableSkillId);
        });

        var byUser = db.GetUserSkillIds(state.RemovableSkillId, state.PoolUserIds);
        state.RemovableUserSkillIds.Clear();
        foreach (var userId in state.PoolUserIds)
        {
            state.RemovableUserSkillIds.Add(
                byUser.TryGetValue(userId, out var userSkillId) ? userSkillId : Guid.Empty);
        }

        state.RemovableUserSkillId = state.RemovableUserSkillIds.FirstOrDefault(id => id != Guid.Empty);
        Console.WriteLine($"[seed] {byUser.Count} removable user-skills assigned.");
    }

    private async Task<Guid?> AddSkillAsync(string token, Guid skillId)
    {
        var (status, body) = await client.PostJsonAsync(
            "/api/v1/profiles/me/skills",
            new { skillId, type = 1, proficiencyLevel = 3, yearsOfExperience = 5 },
            token);

        if (status is not (200 or 201))
        {
            return null;
        }

        return SeedState.JsonGuid(SeedState.ParseJson(body), "id");
    }

    private async Task CreateLifecycleSwapsAsync(SeedState state)
    {
        state.AcceptedSwapId = await CreateSwapAsync(state, state.BobId, state.RequestedSkillId);
        await TransitionAsync($"/api/v1/swap-requests/{state.AcceptedSwapId}/accept", state.BobToken);

        state.EndableSwapId = await CreateSwapAsync(state, state.BobId, state.RequestedSkillId);
        await TransitionAsync($"/api/v1/swap-requests/{state.EndableSwapId}/accept", state.BobToken);

        state.CompletedSwapId = await CreateSwapAsync(state, state.BobId, state.RequestedSkillId);
        await TransitionAsync($"/api/v1/swap-requests/{state.CompletedSwapId}/accept", state.BobToken);
        await TransitionAsync($"/api/v1/swap-requests/{state.CompletedSwapId}/complete", state.AliceToken);
        await TransitionAsync($"/api/v1/swap-requests/{state.CompletedSwapId}/complete", state.BobToken);

        state.RejectedSwapId = await CreateSwapAsync(state, state.BobId, state.RequestedSkillId);
        await TransitionAsync($"/api/v1/swap-requests/{state.RejectedSwapId}/reject", state.BobToken);

        state.ProposalSwapId = await CreateSwapAsync(state, state.BobId, state.RequestedSkillId);
        await TransitionAsync($"/api/v1/swap-requests/{state.ProposalSwapId}/accept", state.BobToken);

        var scheduledDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);
        var (proposalStatus, proposalBody) = await client.PostJsonAsync(
            $"/api/v1/swap-requests/{state.ProposalSwapId}/proposals",
            new
            {
                scheduledDate = scheduledDate.ToString("yyyy-MM-dd"),
                startTime = "18:00",
                endTime = "19:00",
                durationMinutes = 60
            },
            state.AliceToken);

        state.ProposalId = proposalStatus is (200 or 201)
            ? SeedState.JsonGuid(SeedState.ParseJson(proposalBody), "id") ?? Guid.Empty
            : Guid.Empty;

        if (state.ProposalId == Guid.Empty)
        {
            Console.WriteLine($"[seed] proposal creation -> {proposalStatus} {Truncate(proposalBody)}");
        }

        var (joinStatus, joinBody) = await client.PostJsonAsync(
            $"/api/v1/live-sessions/{state.AcceptedSwapId}/join", null!, state.AliceToken);
        state.LiveSessionRoomId = joinStatus == 200
            ? SeedState.JsonGuid(SeedState.ParseJson(joinBody), "id") ?? Guid.Empty
            : Guid.Empty;
        Console.WriteLine($"[seed] live session join -> {joinStatus}, room {state.LiveSessionRoomId}");

        var (endJoinStatus, endJoinBody) = await client.PostJsonAsync(
            $"/api/v1/live-sessions/{state.EndableSwapId}/join", null!, state.AliceToken);
        state.EndableRoomId = endJoinStatus == 200
            ? SeedState.JsonGuid(SeedState.ParseJson(endJoinBody), "id") ?? Guid.Empty
            : Guid.Empty;

        state.CancelGuardSwapId = await CreateSwapAsync(state, state.BobId, state.RequestedSkillId);
        state.ConversationId = db.FindConversationId(state.AcceptedSwapId) ?? Guid.Empty;
        Console.WriteLine($"[seed] conversation {state.ConversationId}");
    }

    private async Task<Guid> CreateSwapAsync(SeedState state, Guid receiverId, Guid requestedSkillId)
    {
        var (status, body) = await client.PostJsonAsync(
            "/api/v1/swap-requests",
            new
            {
                receiverId,
                offeredSkillId = state.OfferedSkillId,
                requestedSkillId,
                proposedScheduleDetails = "Weekday evenings"
            },
            state.AliceToken);

        if (status is not (200 or 201))
        {
            throw new InvalidOperationException($"Swap creation failed: {status} {Truncate(body)}");
        }

        return SeedState.JsonGuid(SeedState.ParseJson(body), "id")
               ?? throw new InvalidOperationException($"No swap id in {Truncate(body)}");
    }

    private async Task TransitionAsync(string path, string token)
    {
        var (status, body) = await client.PutJsonAsync(path, null, token);
        if (status is not (200 or 204))
        {
            Console.WriteLine($"[seed] {path} -> {status} {Truncate(body)}");
        }
    }

    private Task SeedBulkDataAsync(SeedState state)
    {
        var count = options.Concurrency;
        var pool = state.PoolUserIds;

        // Helper: build the parallel receiver list for a set of seeded swap ids.
        // The seeder cycles pool[i % pool.Count] as receiver for swap i.
        List<Guid> ReceiverList(List<Guid> swapIds) =>
            swapIds.Select((_, i) => pool.Count == 0 ? state.BobId : pool[i % pool.Count]).ToList();

        state.PendingForAcceptIds.AddRange(db.SeedSwaps(
            $"load.accept.{state.RunStamp}", state.AliceId, pool, state.OfferedSkillId, BulkSkillA,
            SwapRequestStatus.Pending, false, false, count));
        state.PendingForAcceptReceiverIds.AddRange(ReceiverList(state.PendingForAcceptIds));

        state.PendingForRejectIds.AddRange(db.SeedSwaps(
            $"load.reject.{state.RunStamp}", state.AliceId, pool, state.OfferedSkillId, BulkSkillB,
            SwapRequestStatus.Pending, false, false, count));
        state.PendingForRejectReceiverIds.AddRange(ReceiverList(state.PendingForRejectIds));

        state.PendingForCancelIds.AddRange(db.SeedSwaps(
            $"load.cancel.{state.RunStamp}", state.AliceId, pool, state.OfferedSkillId, BulkSkillC,
            SwapRequestStatus.Pending, false, false, count));

        state.AcceptedForCompleteIds.AddRange(db.SeedSwaps(
            $"load.complete.{state.RunStamp}", state.AliceId, pool, state.OfferedSkillId, BulkSkillD,
            SwapRequestStatus.Accepted, true, false, count));
        state.AcceptedForCompleteReceiverIds.AddRange(ReceiverList(state.AcceptedForCompleteIds));

        state.ReviewableSwapIds.AddRange(db.SeedSwaps(
            $"load.review.{state.RunStamp}", state.AliceId, pool, state.OfferedSkillId, BulkSkillE,
            SwapRequestStatus.Completed, true, true, count));
        state.ReviewableReceiverIds.AddRange(ReceiverList(state.ReviewableSwapIds));

        state.ProposalAcceptSwapIds.AddRange(db.SeedSwaps(
            $"load.proposal-accept.{state.RunStamp}", state.AliceId, pool, state.OfferedSkillId, BulkSkillA,
            SwapRequestStatus.Accepted, false, false, count));
        state.ProposalAcceptReceiverIds.AddRange(ReceiverList(state.ProposalAcceptSwapIds));
        state.ProposalAcceptIds.AddRange(db.SeedProposals(state.ProposalAcceptSwapIds, state.AliceId));

        state.ProposalRejectSwapIds.AddRange(db.SeedSwaps(
            $"load.proposal-reject.{state.RunStamp}", state.AliceId, pool, state.OfferedSkillId, BulkSkillB,
            SwapRequestStatus.Accepted, false, false, count));
        state.ProposalRejectReceiverIds.AddRange(ReceiverList(state.ProposalRejectSwapIds));
        state.ProposalRejectIds.AddRange(db.SeedProposals(state.ProposalRejectSwapIds, state.AliceId));

        state.ProposalCreateSwapIds.AddRange(db.SeedSwaps(
            $"load.proposal-create.{state.RunStamp}", state.AliceId, pool, state.OfferedSkillId, BulkSkillC,
            SwapRequestStatus.Accepted, false, false, count));

        state.CreditPackageId = db.SeedCreditPackages();
        state.AliceWalletId = db.SeedWallet(state.AliceId, 100000);
        db.SeedWallet(state.BobId, 100000);
        foreach (var userId in pool.Take(50))
        {
            db.SeedWallet(userId, 600);
        }

        state.WalletTransactionIds.AddRange(
            db.SeedWalletTransactions(state.AliceWalletId, state.BobId, Math.Max(count, 50)));

        return Task.CompletedTask;
    }

    private static async Task RunParallelAsync(int count, Func<int, Task> action)
    {
        if (count <= 0)
        {
            return;
        }

        using var gate = new SemaphoreSlim(SeedParallelism);
        var tasks = new List<Task>(count);
        for (var index = 0; index < count; index++)
        {
            var captured = index;
            await gate.WaitAsync();
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    await action(captured);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[seed] step {captured} failed: {ex.Message}");
                }
                finally
                {
                    gate.Release();
                }
            }));
        }

        await Task.WhenAll(tasks);
    }

    private static string Truncate(string content) =>
        content.Length <= 300 ? content : content[..300] + "...";
}
