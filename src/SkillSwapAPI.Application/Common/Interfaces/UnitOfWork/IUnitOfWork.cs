using Microsoft.EntityFrameworkCore.Storage;
using SkillSwapAPI.Application.Common.Interfaces.Repos;
using SkillSwapAPI.Domain.Modules.Chat.Entities;

namespace SkillSwapAPI.Application.Common.Interfaces.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IRefreshTokenRepository RefreshTokens { get; }
        ISkillCategoryRepository SkillCategories { get; }
        ISkillRepository Skills { get; }
        IUserSkillRepository UserSkills { get; }
        IUserAvailabilityRepository UserAvailabilities { get; }
        ISwapRequestRepository SwapRequests { get; }
        IConversationRepository Conversations { get; }
        IMessageRepository Messages { get; }
        IReviewRepository Reviews { get; }
        IAuditLogRepository AuditLogs { get; }
        ITimeWalletRepository TimeWallets { get; }
        ITimeLedgerTransactionRepository TimeLedgerTransactions { get; }
        ICreditPackageRepository CreditPackages { get; }
        IPaymentOrderRepository PaymentOrders { get; }
        ILiveSessionRoomRepository LiveSessionRooms { get; }
        ISessionProposalRepository SessionProposals { get; }
        IWhiteboardSnapshotRepository WhiteboardSnapshots { get; }
        IBadgeRepository Badges { get; }
        IUserBadgeAwardRepository UserBadgeAwards { get; }
        Task<int> CompleteAsync(CancellationToken cancellationToken = default);
        Task<IDbContextTransaction> BeginTransactionAsync(
    CancellationToken cancellationToken = default);
    }

}
