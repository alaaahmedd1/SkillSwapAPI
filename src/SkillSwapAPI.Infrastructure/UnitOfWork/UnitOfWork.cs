using SkillSwapAPI.Application.Common.Interfaces.Repos;
using SkillSwapAPI.Application.Common.Interfaces.UnitOfWork;
using SkillSwapAPI.Domain.Modules.Chat.Entities;
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
            SwapRequests = new SwapRequestRepository(_context);
            Conversations = new BaseRepository<Conversation>(_context);
        }
        public IRefreshTokenRepository RefreshTokens { get; private set; }
        public ISkillCategoryRepository SkillCategories { get; private set; }
        public ISkillRepository Skills { get; private set; }
        public IUserSkillRepository UserSkills { get; private set; }
        public ISwapRequestRepository SwapRequests { get; private set; }
        public IBaseRepository<Conversation> Conversations { get; private set; }


        public async Task<int> CompleteAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _context.Dispose();
        }
    }
}
