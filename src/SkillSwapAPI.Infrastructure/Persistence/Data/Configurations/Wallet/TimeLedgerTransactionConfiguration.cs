using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkillSwapAPI.Domain.Modules.SwapRequests.Entities;
using SkillSwapAPI.Domain.Modules.Wallet.Entities;
using SkillSwapAPI.Infrastructure.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkillSwapAPI.Infrastructure.Persistence.Data.Configurations.Wallet
{
        internal sealed class TimeLedgerTransactionConfiguration
       : IEntityTypeConfiguration<TimeLedgerTransaction>
        {
            public void Configure(
                EntityTypeBuilder<TimeLedgerTransaction> builder)
            {
                builder.ToTable("TimeLedgerTransactions");

                builder.HasKey(transaction => transaction.Id);

                builder.Property(transaction => transaction.WalletId)
                    .IsRequired();

                builder.Property(transaction => transaction.TransactionType)
                    .HasConversion<int>()
                    .IsRequired();

                builder.Property(transaction => transaction.AmountMinutes)
                    .IsRequired();

                builder.Property(transaction => transaction.RunningBalanceMinutes)
                    .IsRequired();

                builder.Property(transaction => transaction.ReferenceCode)
                    .HasMaxLength(50)
                    .IsRequired();

                builder.HasIndex(transaction => transaction.ReferenceCode)
                    .IsUnique();

                builder.HasIndex(transaction => new
                {
                    transaction.SwapRequestId,
                    transaction.TransactionType
                })
                    .IsUnique()
                    .HasFilter("[SwapRequestId] IS NOT NULL");

            builder.Property(transaction => transaction.Title)
                    .HasMaxLength(200)
                    .IsRequired();

                builder.Property(transaction => transaction.CreatedAtUtc)
                    .IsRequired();

                builder.HasOne<TimeWallet>()
                    .WithMany()
                    .HasForeignKey(transaction => transaction.WalletId)
                    .OnDelete(DeleteBehavior.Restrict);

                builder.HasOne<SwapRequest>()
                    .WithMany()
                    .HasForeignKey(transaction => transaction.SwapRequestId)
                    .OnDelete(DeleteBehavior.Restrict);

                builder.HasOne<AppUser>()
                    .WithMany()
                    .HasForeignKey(transaction => transaction.PartnerUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            }
        }
 
}
