using SkillSwapAPI.Domain.Modules.Chat.Entities;
using SkillSwapAPI.Domain.Modules.SwapRequests.Entities;

namespace SkillSwapAPI.Application.Common.Interfaces.Repos
{
    public interface IConversationRepository : IBaseRepository<Conversation>
    {
        Task<SwapRequest?> GetSwapRequestByConversationAsync(Guid conversationId, CancellationToken cancellationToken = default);
    }
}
