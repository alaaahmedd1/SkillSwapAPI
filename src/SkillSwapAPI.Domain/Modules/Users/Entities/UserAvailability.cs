using SkillSwapAPI.Domain.Modules.Users.Enums;
using System;

namespace SkillSwapAPI.Domain.Modules.Users.Entities
{
    public sealed class UserAvailability
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public TimeBlock TimeBlock { get; set; }
    }
}
