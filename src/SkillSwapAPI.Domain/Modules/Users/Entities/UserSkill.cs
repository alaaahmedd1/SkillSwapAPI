using SkillSwapAPI.Domain.Modules.Users.Enums;
using SkillSwapAPI.Domain.Skills.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkillSwapAPI.Domain.Modules.Users.Entities
{

    public sealed class UserSkill
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid SkillId { get; set; }
        public SkillType Type { get; set; }
        public ProficiencyLevel ProficiencyLevel { get; set; }
        public int? YearsOfExperience { get; set; }

        public Skill Skill { get; set; } = null!;
    }
}
