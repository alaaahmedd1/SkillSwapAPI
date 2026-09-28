using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkillSwapAPI.Domain.Modules.Chat.Entities
{
    public sealed class Conversation
    {
        public Guid Id { get; set; }
        public Guid SwapRequestId { get; set; }
        public DateTimeOffset CreatedAtUtc { get; set; }
    }
}
