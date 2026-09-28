using Microsoft.EntityFrameworkCore;
using SkillSwapAPI.Application.Common.Interfaces.Repos;
using SkillSwapAPI.Domain.Modules.Chat.Entities;
using SkillSwapAPI.Domain.Modules.SwapRequests.Entities;
using SkillSwapAPI.Infrastructure.Persistence.Data.DbContext;

namespace SkillSwapAPI.Infrastructure.Repositories
{
    public class ConversationRepository(
        ApplicationDbContext context) : BaseRepository<Conversation>(context), IConversationRepository
    {
        private readonly ApplicationDbContext _context = context;

        public async Task<SwapRequest?> GetSwapRequestByConversationAsync(Guid conversationId, CancellationToken cancellationToken = default)
        {
            return await (
                from conversation in _context.Set<Conversation>().AsNoTracking()
                join swapRequest in _context.Set<SwapRequest>().AsNoTracking()
                    on conversation.SwapRequestId equals swapRequest.Id
                where conversation.Id == conversationId
                select swapRequest
            ).FirstOrDefaultAsync(cancellationToken);
        }
    }
}
