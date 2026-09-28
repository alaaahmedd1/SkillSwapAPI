using Microsoft.EntityFrameworkCore;
using SkillSwapAPI.Application.Common.Interfaces.Repos;
using SkillSwapAPI.Domain.Modules.Chat.Entities;
using SkillSwapAPI.Infrastructure.Persistence.Data.DbContext;

namespace SkillSwapAPI.Infrastructure.Repositories
{
    public class MessageRepository(
        ApplicationDbContext context) : BaseRepository<Message>(context), IMessageRepository
    {
        private readonly ApplicationDbContext _context = context;

        public async Task<(IReadOnlyList<Message> Items, int TotalCount)> GetPagedByConversationAsync(
            Guid conversationId,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Set<Message>()
                .AsNoTracking()
                .Where(message => message.ConversationId == conversationId);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(message => message.SentAtUtc)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }
    }
}
