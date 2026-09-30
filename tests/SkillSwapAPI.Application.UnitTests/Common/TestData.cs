using SkillSwapAPI.Application.Features.Identity.Dtos;
using SkillSwapAPI.Application.Features.Users.Dtos;
using SkillSwapAPI.Domain.Modules.Badges.Entities;
using SkillSwapAPI.Domain.Modules.Chat.Entities;
using SkillSwapAPI.Domain.Modules.LiveSessions.Entities;
using SkillSwapAPI.Domain.Modules.LiveSessions.Enums;
using SkillSwapAPI.Domain.Modules.Payments.Entities;
using SkillSwapAPI.Domain.Modules.Payments.Enums;
using SkillSwapAPI.Domain.Modules.Reviews.Entities;
using SkillSwapAPI.Domain.Modules.SessionProposals.Entities;
using SkillSwapAPI.Domain.Modules.SessionProposals.Enums;
using SkillSwapAPI.Domain.Modules.SwapRequests.Entities;
using SkillSwapAPI.Domain.Modules.SwapRequests.Enums;
using SkillSwapAPI.Domain.Modules.Users.Entities;
using SkillSwapAPI.Domain.Modules.Users.Enums;
using SkillSwapAPI.Domain.Modules.Wallet.Entities;
using SkillSwapAPI.Domain.Modules.Wallet.Enums;
using SkillSwapAPI.Domain.Skills.Entities;

namespace SkillSwapAPI.Application.UnitTests.Common;

/// <summary>
/// Factory methods producing valid domain entities and identity DTOs with sensible defaults.
/// Every value can be overridden; ids are stable per call site via optional parameters.
/// </summary>
public static class TestData
{
    public static Guid UserId { get; } = Guid.NewGuid();
    public static Guid OtherUserId { get; } = Guid.NewGuid();

    public static SkillCategory Category(int id = 1, string name = "Programming") => new()
    {
        Id = id,
        Name = name,
        IsActive = true
    };

    public static Skill Skill(Guid? id = null, string name = "C#", int categoryId = 1, SkillCategory? category = null) => new()
    {
        Id = id ?? Guid.NewGuid(),
        Name = name,
        CategoryId = categoryId,
        Category = category ?? Category(categoryId)
    };

    public static UserSkill UserSkill(Guid? id = null, Guid? userId = null, Guid? skillId = null,
        SkillType type = SkillType.Offered, ProficiencyLevel level = ProficiencyLevel.Intermediate,
        Skill? skill = null) => new()
    {
        Id = id ?? Guid.NewGuid(),
        UserId = userId ?? UserId,
        SkillId = skillId ?? (skill?.Id ?? Guid.NewGuid()),
        Type = type,
        ProficiencyLevel = level,
        Skill = skill ?? Skill(skillId)
    };

    public static SwapRequest SwapRequest(
        Guid? id = null, Guid? requesterId = null, Guid? receiverId = null,
        Guid? offeredSkillId = null, Guid? requestedSkillId = null,
        SwapRequestStatus status = SwapRequestStatus.Pending,
        bool requesterConfirmed = false, bool receiverConfirmed = false) => new()
    {
        Id = id ?? Guid.NewGuid(),
        RequesterId = requesterId ?? UserId,
        ReceiverId = receiverId ?? OtherUserId,
        OfferedSkillId = offeredSkillId ?? Guid.NewGuid(),
        RequestedSkillId = requestedSkillId ?? Guid.NewGuid(),
        Status = status,
        IsRequesterConfirmed = requesterConfirmed,
        IsReceiverConfirmed = receiverConfirmed,
        ProposedScheduleDetails = "Weekends, afternoon",
        CreatedAtUtc = DateTimeOffset.UtcNow
    };

    public static Conversation Conversation(Guid? id = null, Guid? swapRequestId = null) => new()
    {
        Id = id ?? Guid.NewGuid(),
        SwapRequestId = swapRequestId ?? Guid.NewGuid(),
        CreatedAtUtc = DateTimeOffset.UtcNow
    };

    public static Message Message(Guid? id = null, Guid? conversationId = null, Guid? senderId = null,
        string content = "Hello!", bool isRead = false) => new()
    {
        Id = id ?? Guid.NewGuid(),
        ConversationId = conversationId ?? Guid.NewGuid(),
        SenderId = senderId ?? UserId,
        Content = content,
        IsRead = isRead,
        SentAtUtc = DateTimeOffset.UtcNow
    };

    public static Review Review(Guid? id = null, Guid? swapRequestId = null, Guid? reviewerId = null,
        Guid? revieweeId = null, int rating = 5, string? comment = "Great session") => new()
    {
        Id = id ?? Guid.NewGuid(),
        SwapRequestId = swapRequestId ?? Guid.NewGuid(),
        ReviewerId = reviewerId ?? UserId,
        RevieweeId = revieweeId ?? OtherUserId,
        Rating = rating,
        Comment = comment,
        CreatedAtUtc = DateTimeOffset.UtcNow
    };

    public static Badge Badge(int id = 1, string name = "Super Patient", bool isActive = true) => new()
    {
        Id = id,
        Name = name,
        Description = "Displayed patience during sessions",
        IconUrl = "https://cdn.skillswap.local/badges/patient.png",
        IsActive = isActive
    };

    public static UserBadgeAward UserBadgeAward(Guid? id = null, Guid? reviewId = null, int badgeId = 1,
        Guid? reviewerId = null, Guid? revieweeId = null) => new()
    {
        Id = id ?? Guid.NewGuid(),
        ReviewId = reviewId ?? Guid.NewGuid(),
        BadgeId = badgeId,
        ReviewerId = reviewerId ?? UserId,
        RevieweeId = revieweeId ?? OtherUserId,
        AwardedAtUtc = DateTimeOffset.UtcNow
    };

    public static LiveSessionRoom LiveSessionRoom(Guid? id = null, Guid? swapRequestId = null,
        LiveSessionStatus status = LiveSessionStatus.Waiting,
        DateTimeOffset? actualStartTime = null, DateTimeOffset? actualEndTime = null,
        int durationSeconds = 0) => new()
    {
        Id = id ?? Guid.NewGuid(),
        SwapRequestId = swapRequestId ?? Guid.NewGuid(),
        RoomToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray()),
        ScheduledStartTime = DateTimeOffset.UtcNow,
        Status = status,
        ActualStartTime = actualStartTime,
        ActualEndTime = actualEndTime,
        DurationSeconds = durationSeconds
    };

    public static WhiteboardSnapshot WhiteboardSnapshot(Guid? id = null, Guid? roomId = null,
        string canvas = """{"elements":[]}""") => new()
    {
        Id = id ?? Guid.NewGuid(),
        RoomId = roomId ?? Guid.NewGuid(),
        CanvasDataJson = canvas,
        UpdatedAtUtc = DateTimeOffset.UtcNow
    };

    public static TimeWallet TimeWallet(Guid? id = null, Guid? userId = null, int balanceMinutes = 120,
        int totalEarned = 200, int totalSpent = 80) => new()
    {
        Id = id ?? Guid.NewGuid(),
        UserId = userId ?? UserId,
        BalanceMinutes = balanceMinutes,
        TotalEarnedMinutes = totalEarned,
        TotalSpentMinutes = totalSpent,
        CreatedAtUtc = DateTimeOffset.UtcNow
    };

    public static TimeLedgerTransaction LedgerTransaction(Guid? id = null, Guid? walletId = null,
        TransactionType type = TransactionType.Earned, int amountMinutes = 60, int runningBalance = 120,
        Guid? swapRequestId = null, Guid? partnerUserId = null) => new()
    {
        Id = id ?? Guid.NewGuid(),
        WalletId = walletId ?? Guid.NewGuid(),
        SwapRequestId = swapRequestId,
        TransactionType = type,
        AmountMinutes = amountMinutes,
        RunningBalanceMinutes = runningBalance,
        ReferenceCode = $"TX-{Guid.NewGuid():N}"[..20],
        Title = "Session settlement",
        PartnerUserId = partnerUserId ?? OtherUserId,
        CreatedAtUtc = DateTimeOffset.UtcNow
    };

    public static CreditPackage CreditPackage(Guid? id = null, string name = "Starter Pack", int credits = 120,
        decimal price = 9.99m, bool isActive = true) => new()
    {
        Id = id ?? Guid.NewGuid(),
        Name = name,
        Description = "120 minutes of skill exchange time",
        CreditsCount = credits,
        Price = price,
        Currency = "USD",
        IsActive = isActive
    };

    public static PaymentOrder PaymentOrder(Guid? id = null, Guid? userId = null, Guid? packageId = null,
        PaymentStatus status = PaymentStatus.Pending, CreditPackage? package = null) => new()
    {
        Id = id ?? Guid.NewGuid(),
        UserId = userId ?? UserId,
        CreditPackageId = packageId ?? (package?.Id ?? Guid.NewGuid()),
        CreditPackage = package ?? CreditPackage(packageId),
        Amount = package?.Price ?? 9.99m,
        Currency = "USD",
        Status = status,
        CreatedAtUtc = DateTimeOffset.UtcNow
    };

    public static SessionProposal SessionProposal(Guid? id = null, Guid? swapRequestId = null, Guid? proposerId = null,
        ProposalStatus status = ProposalStatus.Proposed) => new()
    {
        Id = id ?? Guid.NewGuid(),
        SwapRequestId = swapRequestId ?? Guid.NewGuid(),
        ProposerId = proposerId ?? UserId,
        ScheduledDate = DateOnly.FromDateTime(DateTimeOffset.UtcNow.Date),
        StartTime = new TimeOnly(10, 0),
        EndTime = new TimeOnly(11, 0),
        DurationMinutes = 60,
        Status = status,
        CreatedAtUtc = DateTimeOffset.UtcNow
    };

    public static AppUserDto AppUser(Guid? userId = null, string email = "user@test.local", bool isActive = true,
        string firstName = "Ada", string lastName = "Lovelace") => new(
        userId ?? UserId,
        email,
        firstName,
        lastName,
        AverageRating: 4.5m,
        TotalReviewsCount: 10,
        IsActive: isActive,
        CreatedAtUtc: DateTimeOffset.UtcNow,
        Roles: ["User"],
        Claims: []);

    public static ProfileIdentityDto Profile(Guid? userId = null, string firstName = "Ada", string lastName = "Lovelace",
        string email = "user@test.local", bool isActive = true, decimal averageRating = 4.5m,
        int totalReviews = 10) => new(
        userId ?? UserId,
        firstName,
        lastName,
        email,
        averageRating,
        totalReviews,
        isActive,
        DateTimeOffset.UtcNow);
}
