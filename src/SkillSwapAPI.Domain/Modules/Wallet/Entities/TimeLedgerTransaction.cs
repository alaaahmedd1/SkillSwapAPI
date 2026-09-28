using SkillSwapAPI.Domain.Modules.Wallet.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkillSwapAPI.Domain.Modules.Wallet.Entities
{
    public sealed class TimeLedgerTransaction
    {
        public Guid Id { get; set; }

        public Guid WalletId { get; set; }

        public Guid? SwapRequestId { get; set; }

        public TransactionType TransactionType { get; set; }

        public int AmountMinutes { get; set; }

        public int RunningBalanceMinutes { get; set; }

        public string ReferenceCode { get; set; } = null!;

        public string Title { get; set; } = null!;

        public Guid? PartnerUserId { get; set; }

        public DateTimeOffset CreatedAtUtc { get; set; }
    }
    }
