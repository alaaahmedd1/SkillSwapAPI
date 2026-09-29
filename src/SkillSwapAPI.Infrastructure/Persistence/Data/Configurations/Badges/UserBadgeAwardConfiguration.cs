using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkillSwapAPI.Domain.Modules.Badges.Entities;
using SkillSwapAPI.Domain.Modules.Reviews.Entities;
using SkillSwapAPI.Infrastructure.Identity;

namespace SkillSwapAPI.Infrastructure.Persistence.Data.Configurations.Badges;

public sealed class UserBadgeAwardConfiguration : IEntityTypeConfiguration<UserBadgeAward>
{
    public void Configure(EntityTypeBuilder<UserBadgeAward> builder)
    {
        builder.ToTable("UserBadgeAwards");
        builder.HasKey(award => award.Id);
        builder.HasIndex(award => award.ReviewId).IsUnique();
        builder.HasIndex(award => award.RevieweeId);
        builder.HasOne<Review>()
            .WithMany()
            .HasForeignKey(award => award.ReviewId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<Badge>()
            .WithMany()
            .HasForeignKey(award => award.BadgeId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(award => award.ReviewerId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(award => award.RevieweeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
