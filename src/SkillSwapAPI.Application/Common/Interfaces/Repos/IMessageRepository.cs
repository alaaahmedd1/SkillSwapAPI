using SkillSwapAPI.Domain.Modules.Chat.Entities;

namespace SkillSwapAPI.Application.Common.Interfaces.Repos
{
    public interface IMessageRepository : IBaseRepository<Message>
    {
        Task<(IReadOnlyList<Message> Items, int TotalCount)> GetPagedByConversationAsync(
            Guid conversationId,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken = default);
    }
}
