using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkillSwapAPI.Domain.Modules.Wallet.Entities
{
    public sealed class TimeWallet
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public int BalanceMinutes { get; set; }

        public int TotalEarnedMinutes { get; set; }

        public int TotalSpentMinutes { get; set; }

        public byte[] RowVersion { get; set; } = [];

        public DateTimeOffset CreatedAtUtc { get; set; }

        public DateTimeOffset? UpdatedAtUtc { get; set; }
    }
}
