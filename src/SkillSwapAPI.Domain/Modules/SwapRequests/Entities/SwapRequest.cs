using SkillSwapAPI.Domain.Modules.SwapRequests.Enums;
using SkillSwapAPI.Domain.Skills.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkillSwapAPI.Domain.Modules.SwapRequests.Entities
{
    public sealed class SwapRequest
    {
        public Guid Id { get; set; }
        public Guid RequesterId { get; set; }
        public Guid ReceiverId { get; set; }
        public Guid OfferedSkillId { get; set; }
        public Guid RequestedSkillId { get; set; }
        public SwapRequestStatus Status { get; set; }
        public bool IsRequesterConfirmed { get; set; }
        public bool IsReceiverConfirmed { get; set; }
        public string? ProposedScheduleDetails { get; set; }
        public byte[] RowVersion { get; set; } = [];
        public DateTimeOffset CreatedAtUtc { get; set; }
        public DateTimeOffset? UpdatedAtUtc { get; set; }

        public Skill OfferedSkill { get; set; } = null!;
        public Skill RequestedSkill { get; set; } = null!;
    }
}
