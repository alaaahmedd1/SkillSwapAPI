using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkillSwapAPI.Domain.Modules.Badges.Entities
{
    public sealed class UserBadgeAward
    {
        public Guid Id { get; set; }

        public Guid ReviewId { get; set; }

        public int BadgeId { get; set; }

        public Guid ReviewerId { get; set; }

        public Guid RevieweeId { get; set; }

        public DateTimeOffset AwardedAtUtc { get; set; }
    }
}
