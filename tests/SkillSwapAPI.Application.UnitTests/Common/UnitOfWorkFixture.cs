using NSubstitute;
using SkillSwapAPI.Application.Common.Interfaces.Repos;
using SkillSwapAPI.Application.Common.Interfaces.UnitOfWork;

namespace SkillSwapAPI.Application.UnitTests.Common;

/// <summary>
/// Creates an IUnitOfWork substitute with every repository pre-wired,
/// so tests only configure the repos they care about. CompleteAsync returns 1 by default.
/// </summary>
public sealed class UnitOfWorkFixture
{
    public IUnitOfWork UnitOfWork { get; }
    public IRefreshTokenRepository RefreshTokens { get; } = Substitute.For<IRefreshTokenRepository>();
    public ISkillCategoryRepository SkillCategories { get; } = Substitute.For<ISkillCategoryRepository>();
    public ISkillRepository Skills { get; } = Substitute.For<ISkillRepository>();
    public IUserSkillRepository UserSkills { get; } = Substitute.For<IUserSkillRepository>();
    public ISwapRequestRepository SwapRequests { get; } = Substitute.For<ISwapRequestRepository>();
    public IConversationRepository Conversations { get; } = Substitute.For<IConversationRepository>();
    public IMessageRepository Messages { get; } = Substitute.For<IMessageRepository>();
    public IReviewRepository Reviews { get; } = Substitute.For<IReviewRepository>();
    public IAuditLogRepository AuditLogs { get; } = Substitute.For<IAuditLogRepository>();
    public ITimeWalletRepository TimeWallets { get; } = Substitute.For<ITimeWalletRepository>();
    public ITimeLedgerTransactionRepository TimeLedgerTransactions { get; } = Substitute.For<ITimeLedgerTransactionRepository>();
    public ICreditPackageRepository CreditPackages { get; } = Substitute.For<ICreditPackageRepository>();
    public IPaymentOrderRepository PaymentOrders { get; } = Substitute.For<IPaymentOrderRepository>();
    public ILiveSessionRoomRepository LiveSessionRooms { get; } = Substitute.For<ILiveSessionRoomRepository>();
    public ISessionProposalRepository SessionProposals { get; } = Substitute.For<ISessionProposalRepository>();
    public IWhiteboardSnapshotRepository WhiteboardSnapshots { get; } = Substitute.For<IWhiteboardSnapshotRepository>();
    public IBadgeRepository Badges { get; } = Substitute.For<IBadgeRepository>();
    public IUserBadgeAwardRepository UserBadgeAwards { get; } = Substitute.For<IUserBadgeAwardRepository>();

    public UnitOfWorkFixture()
    {
        UnitOfWork = Substitute.For<IUnitOfWork>();
        UnitOfWork.RefreshTokens.Returns(RefreshTokens);
        UnitOfWork.SkillCategories.Returns(SkillCategories);
        UnitOfWork.Skills.Returns(Skills);
        UnitOfWork.UserSkills.Returns(UserSkills);
        UnitOfWork.SwapRequests.Returns(SwapRequests);
        UnitOfWork.Conversations.Returns(Conversations);
        UnitOfWork.Messages.Returns(Messages);
        UnitOfWork.Reviews.Returns(Reviews);
        UnitOfWork.AuditLogs.Returns(AuditLogs);
        UnitOfWork.TimeWallets.Returns(TimeWallets);
        UnitOfWork.TimeLedgerTransactions.Returns(TimeLedgerTransactions);
        UnitOfWork.CreditPackages.Returns(CreditPackages);
        UnitOfWork.PaymentOrders.Returns(PaymentOrders);
        UnitOfWork.LiveSessionRooms.Returns(LiveSessionRooms);
        UnitOfWork.SessionProposals.Returns(SessionProposals);
        UnitOfWork.WhiteboardSnapshots.Returns(WhiteboardSnapshots);
        UnitOfWork.Badges.Returns(Badges);
        UnitOfWork.UserBadgeAwards.Returns(UserBadgeAwards);
        UnitOfWork.CompleteAsync(Arg.Any<CancellationToken>()).Returns(1);
    }
}
