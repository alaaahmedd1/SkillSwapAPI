namespace SkillSwapAPI.LoadTests;

public sealed record LoadRequest(string Path, HttpMethod Method, object? Body, string? Token);

public sealed class LoadEndpoint
{
    public required string Name { get; init; }
    public required string Group { get; init; }
    public required string Route { get; init; }
    public required bool Mutating { get; init; }
    public string Notes { get; init; } = string.Empty;
    public required Func<SeedState, int, LoadRequest> Factory { get; init; }
}

public static class EndpointCatalog
{
    public static IReadOnlyList<LoadEndpoint> Build(SeedState s)
    {
        var stamp = s.RunStamp;

        return
        [
            new LoadEndpoint
            {
                Name = "health",
                Group = "Health",
                Route = "GET /api/health",
                Mutating = false,
                Factory = (_, _) => new LoadRequest("/api/health", HttpMethod.Get, null, null)
            },
            new LoadEndpoint
            {
                Name = "guest-feed",
                Group = "GuestFeed",
                Route = "GET /api/guestfeed",
                Mutating = false,
                Notes = "Static sanitized listing data",
                Factory = (_, _) => new LoadRequest("/api/guestfeed?pageNumber=1&pageSize=10", HttpMethod.Get, null, null)
            },
            new LoadEndpoint
            {
                Name = "auth-register",
                Group = "Auth",
                Route = "POST /api/auth/register",
                Mutating = true,
                Notes = "Unique email per request; creates AspNetUser + OTP token",
                Factory = (_, i) => new LoadRequest(
                    "/api/auth/register",
                    HttpMethod.Post,
                    new
                    {
                        firstName = "Load",
                        lastName = $"Runner{i}",
                        email = s.RegisterEmailAt(i),
                        password = SeedState.Password
                    },
                    null)
            },
            new LoadEndpoint
            {
                Name = "auth-login",
                Group = "Auth",
                Route = "POST /api/auth/login",
                Mutating = true,
                Notes = "Repeatable; issues a new refresh token per call",
                Factory = (_, _) => new LoadRequest(
                    "/api/auth/login",
                    HttpMethod.Post,
                    new { email = s.AliceEmail, password = SeedState.Password },
                    null)
            },
            new LoadEndpoint
            {
                Name = "auth-social-login",
                Group = "Auth",
                Route = "POST /api/auth/social-login",
                Mutating = false,
                Notes = "Requires Google IdP - expected to be rejected without a valid id_token",
                Factory = (_, _) => new LoadRequest(
                    "/api/auth/social-login",
                    HttpMethod.Post,
                    new { idToken = "load-test-invalid-id-token", provider = 1 },
                    null)
            },
            new LoadEndpoint
            {
                Name = "auth-refresh-token",
                Group = "Auth",
                Route = "POST /api/auth/refresh-token",
                Mutating = true,
                Notes = "Single-use rotation; distinct seeded token per request",
                Factory = (_, i) =>
                {
                    var pair = s.TokenPairAt(i);
                    return new LoadRequest(
                        "/api/auth/refresh-token",
                        HttpMethod.Post,
                        new { refreshToken = pair.RefreshToken, expiredAccessToken = pair.AccessToken },
                        null);
                }
            },
            new LoadEndpoint
            {
                Name = "auth-forgot-password",
                Group = "Auth",
                Route = "POST /api/auth/forgot-password",
                Mutating = true,
                Notes = "Distinct pool user per request to avoid concurrent OTP duplicate-key races",
                Factory = (_, i) => new LoadRequest(
                    "/api/auth/forgot-password",
                    HttpMethod.Post,
                    new { email = s.PoolEmails.Count == 0 ? s.AliceEmail : s.PoolEmails[i % s.PoolEmails.Count] },
                    null)
            },
            new LoadEndpoint
            {
                Name = "auth-verify-otp",
                Group = "Auth",
                Route = "POST /api/auth/verify-otp",
                Mutating = true,
                Notes = "Single-use; distinct unconfirmed user + real OTP per request",
                Factory = (_, i) => new LoadRequest(
                    "/api/auth/verify-otp",
                    HttpMethod.Post,
                    new { email = s.OtpEmail(i), otp = s.OtpCode(i) },
                    null)
            },
            new LoadEndpoint
            {
                Name = "auth-reset-password",
                Group = "Auth",
                Route = "POST /api/auth/reset-password",
                Mutating = true,
                Notes = "Single-use; distinct user + real OTP per request",
                Factory = (_, i) => new LoadRequest(
                    "/api/auth/reset-password",
                    HttpMethod.Post,
                    new { email = s.ResetEmail(i), otp = s.ResetCode(i), newPassword = SeedState.Password },
                    null)
            },
            new LoadEndpoint
            {
                Name = "auth-logout",
                Group = "Auth",
                Route = "POST /api/auth/logout",
                Mutating = true,
                Notes = "Revokes a refresh token; distinct token per request",
                Factory = (_, i) => new LoadRequest(
                    "/api/auth/logout",
                    HttpMethod.Post,
                    new { refreshToken = s.TokenPairAt(i).RefreshToken },
                    s.PoolToken(i))
            },
            new LoadEndpoint
            {
                Name = "skills-catalog",
                Group = "Skills",
                Route = "GET /api/v1/skills",
                Mutating = false,
                Factory = (_, _) => new LoadRequest("/api/v1/skills", HttpMethod.Get, null, null)
            },
            new LoadEndpoint
            {
                Name = "badges-list",
                Group = "Badges",
                Route = "GET /api/v1/badges",
                Mutating = false,
                Factory = (_, _) => new LoadRequest("/api/v1/badges", HttpMethod.Get, null, null)
            },
            new LoadEndpoint
            {
                Name = "profiles-get-me",
                Group = "Profiles",
                Route = "GET /api/v1/profiles/me",
                Mutating = false,
                Factory = (_, _) => new LoadRequest("/api/v1/profiles/me", HttpMethod.Get, null, s.AliceToken)
            },
            new LoadEndpoint
            {
                Name = "profiles-update-me",
                Group = "Profiles",
                Route = "PUT /api/v1/profiles/me",
                Mutating = true,
                Notes = "Idempotent write of the same profile values",
                Factory = (_, _) => new LoadRequest(
                    "/api/v1/profiles/me",
                    HttpMethod.Put,
                    new { firstName = "Load", lastName = "Alice" },
                    s.AliceToken)
            },
            new LoadEndpoint
            {
                Name = "profiles-add-skill",
                Group = "Profiles",
                Route = "POST /api/v1/profiles/me/skills",
                Mutating = true,
                Notes = "Distinct pool user per request so every insert is new",
                Factory = (_, i) => new LoadRequest(
                    "/api/v1/profiles/me/skills",
                    HttpMethod.Post,
                    new { skillId = s.AddSkillTargetId, type = 1, proficiencyLevel = 2, yearsOfExperience = 3 },
                    s.PoolToken(i))
            },
            new LoadEndpoint
            {
                Name = "profiles-remove-skill",
                Group = "Profiles",
                Route = "DELETE /api/v1/profiles/me/skills/{userSkillId}",
                Mutating = true,
                Notes = "Single-use delete; distinct user-skill per request",
                Factory = (_, i) => new LoadRequest(
                    $"/api/v1/profiles/me/skills/{s.RemovableUserSkillIdAt(i)}",
                    HttpMethod.Delete,
                    null,
                    s.PoolToken(i))
            },
            new LoadEndpoint
            {
                Name = "users-search",
                Group = "Users",
                Route = "GET /api/v1/users/search",
                Mutating = false,
                Factory = (_, _) => new LoadRequest(
                    "/api/v1/users/search?searchTerm=Load&pageNumber=1&pageSize=10",
                    HttpMethod.Get,
                    null,
                    s.AliceToken)
            },
            new LoadEndpoint
            {
                Name = "users-get-reviews",
                Group = "Users",
                Route = "GET /api/v1/users/{userId}/reviews",
                Mutating = false,
                Factory = (_, _) => new LoadRequest(
                    $"/api/v1/users/{s.BobId}/reviews?pageNumber=1&pageSize=10",
                    HttpMethod.Get,
                    null,
                    s.AliceToken)
            },
            new LoadEndpoint
            {
                Name = "users-get-badges",
                Group = "Users",
                Route = "GET /api/v1/users/{userId}/badges",
                Mutating = false,
                Factory = (_, _) => new LoadRequest(
                    $"/api/v1/users/{s.BobId}/badges",
                    HttpMethod.Get,
                    null,
                    s.AliceToken)
            },
            new LoadEndpoint
            {
                Name = "conversations-messages",
                Group = "Conversations",
                Route = "GET /api/v1/conversations/{conversationId}/messages",
                Mutating = false,
                Factory = (_, _) => new LoadRequest(
                    $"/api/v1/conversations/{s.ConversationId}/messages?pageNumber=1&pageSize=50",
                    HttpMethod.Get,
                    null,
                    s.AliceToken)
            },
            new LoadEndpoint
            {
                Name = "swap-create",
                Group = "SwapRequests",
                Route = "POST /api/v1/swap-requests",
                Mutating = true,
                Notes = "Distinct receiver per request to avoid the pending-duplicate rule",
                Factory = (_, i) => new LoadRequest(
                    "/api/v1/swap-requests",
                    HttpMethod.Post,
                    new
                    {
                        receiverId = s.PoolUserId(i),
                        offeredSkillId = s.OfferedSkillId,
                        requestedSkillId = s.LoadRunSkillId,
                        proposedScheduleDetails = "Weekday evenings"
                    },
                    s.AliceToken)
            },
            new LoadEndpoint
            {
                Name = "swap-list",
                Group = "SwapRequests",
                Route = "GET /api/v1/swap-requests",
                Mutating = false,
                Factory = (_, _) => new LoadRequest(
                    "/api/v1/swap-requests?pageNumber=1&pageSize=10",
                    HttpMethod.Get,
                    null,
                    s.AliceToken)
            },
            new LoadEndpoint
            {
                Name = "swap-details",
                Group = "SwapRequests",
                Route = "GET /api/v1/swap-requests/{swapRequestId}",
                Mutating = false,
                Factory = (_, _) => new LoadRequest(
                    $"/api/v1/swap-requests/{s.AcceptedSwapId}",
                    HttpMethod.Get,
                    null,
                    s.AliceToken)
            },
            new LoadEndpoint
            {
                Name = "swap-accept",
                Group = "SwapRequests",
                Route = "PUT /api/v1/swap-requests/{swapRequestId}/accept",
                Mutating = true,
                Notes = "Distinct pending swap per request; uses the matching receiver's token",
                Factory = (_, i) => new LoadRequest(
                    $"/api/v1/swap-requests/{s.BatchId(s.PendingForAcceptIds, i, s.AcceptedSwapId)}/accept",
                    HttpMethod.Put,
                    null,
                    s.BatchReceiverToken(s.PendingForAcceptReceiverIds, i, s.BobToken))
            },
            new LoadEndpoint
            {
                Name = "swap-reject",
                Group = "SwapRequests",
                Route = "PUT /api/v1/swap-requests/{swapRequestId}/reject",
                Mutating = true,
                Notes = "Distinct pending swap per request; uses the matching receiver's token",
                Factory = (_, i) => new LoadRequest(
                    $"/api/v1/swap-requests/{s.BatchId(s.PendingForRejectIds, i, s.RejectedSwapId)}/reject",
                    HttpMethod.Put,
                    null,
                    s.BatchReceiverToken(s.PendingForRejectReceiverIds, i, s.BobToken))
            },
            new LoadEndpoint
            {
                Name = "swap-cancel",
                Group = "SwapRequests",
                Route = "PUT /api/v1/swap-requests/{swapRequestId}/cancel",
                Mutating = true,
                Notes = "Distinct pending swap per request",
                Factory = (_, i) => new LoadRequest(
                    $"/api/v1/swap-requests/{s.BatchId(s.PendingForCancelIds, i, s.CancelGuardSwapId)}/cancel",
                    HttpMethod.Put,
                    null,
                    s.AliceToken)
            },
            new LoadEndpoint
            {
                Name = "swap-complete",
                Group = "SwapRequests",
                Route = "PUT /api/v1/swap-requests/{swapRequestId}/complete",
                Mutating = true,
                Notes = "Pre-confirmed by requester; uses the matching receiver's token",
                Factory = (_, i) => new LoadRequest(
                    $"/api/v1/swap-requests/{s.BatchId(s.AcceptedForCompleteIds, i, s.CompletedSwapId)}/complete",
                    HttpMethod.Put,
                    null,
                    s.BatchReceiverToken(s.AcceptedForCompleteReceiverIds, i, s.BobToken))
            },
            new LoadEndpoint
            {
                Name = "proposal-create",
                Group = "SessionProposals",
                Route = "POST /api/v1/swap-requests/{id}/proposals",
                Mutating = true,
                Notes = "Distinct accepted swap with no active proposal per request",
                Factory = (_, i) => new LoadRequest(
                    $"/api/v1/swap-requests/{s.BatchId(s.ProposalCreateSwapIds, i, s.ProposalSwapId)}/proposals",
                    HttpMethod.Post,
                    new
                    {
                        scheduledDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1).ToString("yyyy-MM-dd"),
                        startTime = "18:00",
                        endTime = "19:00",
                        durationMinutes = 60
                    },
                    s.AliceToken)
            },
            new LoadEndpoint
            {
                Name = "proposal-accept",
                Group = "SessionProposals",
                Route = "PUT /api/v1/swap-requests/{id}/proposals/{proposalId}/accept",
                Mutating = true,
                Notes = "Distinct proposal per request; uses the matching receiver's token",
                Factory = (_, i) => new LoadRequest(
                    $"/api/v1/swap-requests/{s.BatchId(s.ProposalAcceptSwapIds, i, s.ProposalSwapId)}/proposals/" +
                    $"{s.BatchId(s.ProposalAcceptIds, i, s.ProposalId)}/accept",
                    HttpMethod.Put,
                    null,
                    s.BatchReceiverToken(s.ProposalAcceptReceiverIds, i, s.BobToken))
            },
            new LoadEndpoint
            {
                Name = "proposal-reject",
                Group = "SessionProposals",
                Route = "PUT /api/v1/swap-requests/{id}/proposals/{proposalId}/reject",
                Mutating = true,
                Notes = "Distinct proposal per request; uses the matching receiver's token",
                Factory = (_, i) => new LoadRequest(
                    $"/api/v1/swap-requests/{s.BatchId(s.ProposalRejectSwapIds, i, s.ProposalSwapId)}/proposals/" +
                    $"{s.BatchId(s.ProposalRejectIds, i, s.ProposalId)}/reject",
                    HttpMethod.Put,
                    null,
                    s.BatchReceiverToken(s.ProposalRejectReceiverIds, i, s.BobToken))
            },
            new LoadEndpoint
            {
                Name = "live-session-join",
                Group = "LiveSessions",
                Route = "POST /api/v1/live-sessions/{swapId}/join",
                Mutating = true,
                Notes = "Idempotent - returns the existing room token",
                Factory = (_, _) => new LoadRequest(
                    $"/api/v1/live-sessions/{s.AcceptedSwapId}/join",
                    HttpMethod.Post,
                    null,
                    s.AliceToken)
            },
            new LoadEndpoint
            {
                Name = "live-session-end",
                Group = "LiveSessions",
                Route = "POST /api/v1/live-sessions/{roomId}/end",
                Mutating = true,
                Notes = "First call settles the wallet, the rest are idempotent",
                Factory = (_, _) => new LoadRequest(
                    $"/api/v1/live-sessions/{s.EndableRoomId}/end",
                    HttpMethod.Post,
                    null,
                    s.AliceToken)
            },
            new LoadEndpoint
            {
                Name = "review-submit",
                Group = "Reviews",
                Route = "POST /api/v1/reviews",
                Mutating = true,
                Notes = "Distinct completed swap per request; reviewee is the swap receiver; triggers rating recalculation",
                Factory = (_, i) => new LoadRequest(
                    "/api/v1/reviews",
                    HttpMethod.Post,
                    new
                    {
                        swapRequestId = s.ReviewableSwapId(i),
                        revieweeId = s.ReviewableReceiverId(i),
                        rating = (i % 5) + 1,
                        comment = "Load test review",
                        badgeId = (int?)null
                    },
                    s.AliceToken)
            },
            new LoadEndpoint
            {
                Name = "wallet-me",
                Group = "Wallet",
                Route = "GET /api/v1/wallet/me",
                Mutating = false,
                Factory = (_, _) => new LoadRequest("/api/v1/wallet/me", HttpMethod.Get, null, s.AliceToken)
            },
            new LoadEndpoint
            {
                Name = "wallet-transactions",
                Group = "Wallet",
                Route = "GET /api/v1/wallet/transactions",
                Mutating = false,
                Factory = (_, _) => new LoadRequest(
                    $"/api/v1/wallet/transactions?userId={s.AliceId}&pageNumber=1&pageSize=10",
                    HttpMethod.Get,
                    null,
                    s.AliceToken)
            },
            new LoadEndpoint
            {
                Name = "wallet-transaction-details",
                Group = "Wallet",
                Route = "GET /api/v1/wallet/transactions/{id}",
                Mutating = false,
                Factory = (_, i) => new LoadRequest(
                    $"/api/v1/wallet/transactions/{s.WalletTransactionId(i)}",
                    HttpMethod.Get,
                    null,
                    s.AliceToken)
            },
            new LoadEndpoint
            {
                Name = "wallet-transaction-receipt",
                Group = "Wallet",
                Route = "GET /api/v1/wallet/transactions/{id}/receipt",
                Mutating = false,
                Notes = "Generates a PDF per request (QuestPDF) - CPU heavy",
                Factory = (_, i) => new LoadRequest(
                    $"/api/v1/wallet/transactions/{s.WalletTransactionId(i)}/receipt",
                    HttpMethod.Get,
                    null,
                    s.AliceToken)
            },
            new LoadEndpoint
            {
                Name = "wallet-receipt-email",
                Group = "Wallet",
                Route = "POST /api/v1/wallet/transactions/{id}/receipt/email",
                Mutating = true,
                Notes = "Queues a receipt email; SMTP is unconfigured in the load profile",
                Factory = (_, i) => new LoadRequest(
                    $"/api/v1/wallet/transactions/{s.WalletTransactionId(i)}/receipt/email",
                    HttpMethod.Post,
                    null,
                    s.AliceToken)
            },
            new LoadEndpoint
            {
                Name = "payments-packages",
                Group = "Payments",
                Route = "GET /api/v1/payments/packages",
                Mutating = false,
                Factory = (_, _) => new LoadRequest("/api/v1/payments/packages", HttpMethod.Get, null, s.AliceToken)
            },
            new LoadEndpoint
            {
                Name = "payments-checkout",
                Group = "Payments",
                Route = "POST /api/v1/payments/checkout",
                Mutating = true,
                Notes = "Calls Stripe - fails without an API key in the load profile",
                Factory = (_, _) => new LoadRequest(
                    "/api/v1/payments/checkout",
                    HttpMethod.Post,
                    new { packageId = s.CreditPackageId, userId = s.AliceId },
                    s.AliceToken)
            },
            new LoadEndpoint
            {
                Name = "payments-webhook",
                Group = "Payments",
                Route = "POST /api/v1/payments/webhook",
                Mutating = true,
                Notes = "Anonymous; rejected without a valid Stripe-Signature",
                Factory = (_, _) => new LoadRequest(
                    "/api/v1/payments/webhook",
                    HttpMethod.Post,
                    new { type = "payment_intent.succeeded" },
                    null)
            },
            new LoadEndpoint
            {
                Name = "admin-create-category",
                Group = "Admin",
                Route = "POST /api/v1/admin/categories",
                Mutating = true,
                Notes = "Unique name per request; writes an audit log row",
                Factory = (_, i) => new LoadRequest(
                    "/api/v1/admin/categories",
                    HttpMethod.Post,
                    new { name = $"Load Run Category {stamp} {i}", description = "Load test" },
                    s.AdminToken)
            },
            new LoadEndpoint
            {
                Name = "admin-create-skill",
                Group = "Admin",
                Route = "POST /api/v1/admin/skills",
                Mutating = true,
                Notes = "Unique name per request; writes an audit log row",
                Factory = (_, i) => new LoadRequest(
                    "/api/v1/admin/skills",
                    HttpMethod.Post,
                    new { categoryId = s.CategoryId, name = $"Load Run Skill {stamp} {i}", description = "Load test" },
                    s.AdminToken)
            },
            new LoadEndpoint
            {
                Name = "admin-list-users",
                Group = "Admin",
                Route = "GET /api/v1/admin/users",
                Mutating = false,
                Factory = (_, _) => new LoadRequest(
                    "/api/v1/admin/users?pageNumber=1&pageSize=10",
                    HttpMethod.Get,
                    null,
                    s.AdminToken)
            },
            new LoadEndpoint
            {
                Name = "admin-update-user-status",
                Group = "Admin",
                Route = "PUT /api/v1/admin/users/{userId}/status",
                Mutating = true,
                Notes = "Suspends the dedicated hammer user; cascades token revocation and swap cancellation",
                Factory = (_, _) => new LoadRequest(
                    $"/api/v1/admin/users/{s.HammerId}/status",
                    HttpMethod.Put,
                    new { isActive = false },
                    s.AdminToken)
            }
        ];
    }
}
