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
        ISwapRequestRepository SwapRequests { get; }
        IConversationRepository Conversations { get; }
        IMessageRepository Messages { get; }
        IReviewRepository Reviews { get; }
        IAuditLogRepository AuditLogs { get; }
        ITimeWalletRepository TimeWallets { get; }
        Task<int> CompleteAsync(CancellationToken cancellationToken = default);
    }

}
