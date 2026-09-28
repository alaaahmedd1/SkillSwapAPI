using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkillSwapAPI.Domain.Modules.Wallet.Entities;
using SkillSwapAPI.Infrastructure.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkillSwapAPI.Infrastructure.Persistence.Data.Configurations.Wallet
{
    internal class TimeWalletConfiguration:IEntityTypeConfiguration<TimeWallet>
    {
        public void Configure(EntityTypeBuilder<TimeWallet> builder)
        {
            builder.ToTable("TimeWallets");

            builder.HasKey(wallet => wallet.Id);

            builder.Property(wallet => wallet.UserId)
                .IsRequired();

            builder.Property(wallet => wallet.BalanceMinutes)
                .HasDefaultValue(0)
                .IsRequired();

            builder.Property(wallet => wallet.TotalEarnedMinutes)
                .HasDefaultValue(0)
                .IsRequired();

            builder.Property(wallet => wallet.TotalSpentMinutes)
                .HasDefaultValue(0)
                .IsRequired();

            builder.Property(wallet => wallet.RowVersion)
                .IsRowVersion();

            builder.Property(wallet => wallet.CreatedAtUtc)
                .IsRequired();

            builder.HasIndex(wallet => wallet.UserId)
                .IsUnique();

            builder.HasOne<AppUser>()
                .WithMany()
                .HasForeignKey(wallet => wallet.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
