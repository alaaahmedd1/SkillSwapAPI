using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkillSwapAPI.Domain.Modules.Badges.Entities
{
    public sealed class Badge
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string Description { get; set; } = null!;

        public string IconUrl { get; set; } = null!;

        public bool IsActive { get; set; }
    }
}
