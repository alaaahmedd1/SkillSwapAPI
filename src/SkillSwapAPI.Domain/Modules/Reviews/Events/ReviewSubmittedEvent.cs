using SkillSwapAPI.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkillSwapAPI.Domain.Modules.Reviews.Events
{
    public sealed class ReviewSubmittedEvent : DomainEvent
    {
        public Guid ReviewId { get; set; }
        public Guid SwapRequestId { get; set; }
        public Guid ReviewerId { get; set; }
        public Guid RevieweeId { get; set; }
        public int Rating { get; set; }
        public DateTimeOffset CreatedAtUtc { get; set; }
    }
}
