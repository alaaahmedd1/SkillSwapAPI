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
            return await _context.Set<Conversation>()
                .AsNoTracking()
                .Join(
                  _context.Set<SwapRequest>().AsNoTracking(),
                  conv => conv.SwapRequestId,
                  swreq => swreq.Id,
                  (conversation, swapRequest) => new
                  {
                      conversation,
                      swapRequest
                  }
                )
                .Where(x => x.conversation.Id == conversationId)
                .Select(x => x.swapRequest)
                .FirstOrDefaultAsync(cancellationToken);

        }
    }
}
