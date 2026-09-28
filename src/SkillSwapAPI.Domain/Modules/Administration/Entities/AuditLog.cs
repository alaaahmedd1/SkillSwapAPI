using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkillSwapAPI.Domain.Modules.Administration.Entities
{

    public sealed class AuditLog
    {
        public Guid Id { get; set; }
        public Guid AdminId { get; set; }
        public string Action { get; set; } = null!;
        public string TargetEntity { get; set; } = null!;
        public string? TargetEntityId { get; set; }
        public string? Reason { get; set; }
        public DateTimeOffset PerformedAtUtc { get; set; }
    }
}
