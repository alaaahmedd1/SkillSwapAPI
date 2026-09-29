namespace SkillSwapAPI.Infrastructure.Persistence.Data.DbContext;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SkillSwapAPI.Domain.Identity;
using SkillSwapAPI.Domain.Modules.Administration.Entities;
using SkillSwapAPI.Domain.Modules.Badges.Entities;
using SkillSwapAPI.Domain.Modules.Chat.Entities;
using SkillSwapAPI.Domain.Modules.LiveSessions.Entities;
using SkillSwapAPI.Domain.Modules.Payments.Entities;
using SkillSwapAPI.Domain.Modules.Reviews.Entities;
using SkillSwapAPI.Domain.Modules.SwapRequests.Entities;
using SkillSwapAPI.Domain.Modules.Users.Entities;
using SkillSwapAPI.Domain.Modules.Wallet.Entities;
using SkillSwapAPI.Domain.Skills.Entities;
using SkillSwapAPI.Infrastructure.Identity;
using System.Reflection;

public class ApplicationDbContext : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<SkillCategory> SkillCategories => Set<SkillCategory>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<UserSkill> UserSkills => Set<UserSkill>();
    public DbSet<SwapRequest> SwapRequests => Set<SwapRequest>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<TimeWallet> TimeWallets => Set<TimeWallet>();
    public DbSet<TimeLedgerTransaction> TimeLedgerTransactions
    => Set<TimeLedgerTransaction>();
    public DbSet<CreditPackage> CreditPackages => Set<CreditPackage>();
    public DbSet<PaymentOrder> PaymentOrders => Set<PaymentOrder>();
    public DbSet<LiveSessionRoom> LiveSessionRooms => Set<LiveSessionRoom>();
    public DbSet<WhiteboardSnapshot> WhiteboardSnapshots => Set<WhiteboardSnapshot>();
    public DbSet<Badge> Badges => Set<Badge>();
    public DbSet<UserBadgeAward> UserBadgeAwards => Set<UserBadgeAward>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
