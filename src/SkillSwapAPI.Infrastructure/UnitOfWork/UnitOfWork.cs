using Microsoft.EntityFrameworkCore.Storage;
using SkillSwapAPI.Application.Common.Interfaces.Repos;
using SkillSwapAPI.Application.Common.Interfaces.UnitOfWork;
using SkillSwapAPI.Infrastructure.Persistence.Data.DbContext;
using SkillSwapAPI.Infrastructure.Repositories;

namespace SkillSwapAPI.Infrastructure.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            RefreshTokens = new RefreshTokenRepository(_context);
            SkillCategories = new SkillCategoryRepository(_context);
            Skills = new SkillRepository(_context);
            UserSkills = new UserSkillRepository(_context);
            UserAvailabilities = new UserAvailabilityRepository(_context);
            SwapRequests = new SwapRequestRepository(_context);
            Conversations = new ConversationRepository(_context);
            Messages = new MessageRepository(_context);
            Reviews = new ReviewRepository(_context);
            AuditLogs = new AuditLogRepository(_context);
            TimeWallets = new TimeWalletRepository(_context);
            TimeLedgerTransactions = new TimeLedgerTransactionRepository(_context);
            CreditPackages = new CreditPackageRepository(_context);
            PaymentOrders = new PaymentOrderRepository(_context);
            LiveSessionRooms = new LiveSessionRoomRepository(_context);
            SessionProposals = new SessionProposalRepository(_context);
            WhiteboardSnapshots = new WhiteboardSnapshotRepository(_context);
            Badges = new BadgeRepository(_context);
            UserBadgeAwards = new UserBadgeAwardRepository(_context);
        }
        public IRefreshTokenRepository RefreshTokens { get; private set; }
        public ISkillCategoryRepository SkillCategories { get; private set; }
        public ISkillRepository Skills { get; private set; }
        public IUserSkillRepository UserSkills { get; private set; }
        public IUserAvailabilityRepository UserAvailabilities { get; private set; }
        public ISwapRequestRepository SwapRequests { get; private set; }
        public IConversationRepository Conversations { get; private set; }
        public IMessageRepository Messages { get; private set; }
        public IReviewRepository Reviews { get; private set; }
        public IAuditLogRepository AuditLogs { get; private set; }
        public ITimeWalletRepository TimeWallets { get; }
        public ITimeLedgerTransactionRepository TimeLedgerTransactions { get; }
        public ICreditPackageRepository CreditPackages { get; }
        public IPaymentOrderRepository PaymentOrders { get; }
        public ILiveSessionRoomRepository LiveSessionRooms { get; }
        public ISessionProposalRepository SessionProposals { get; }
        public IWhiteboardSnapshotRepository WhiteboardSnapshots { get; }
        public IBadgeRepository Badges { get; }
        public IUserBadgeAwardRepository UserBadgeAwards { get; }

        public async Task<int> CompleteAsync(
            CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync(
            CancellationToken cancellationToken = default)
        {
            return await _context.Database.BeginTransactionAsync(
                cancellationToken);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _context.Dispose();
        }
    }
}
