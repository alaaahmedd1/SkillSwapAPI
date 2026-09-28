using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkillSwapAPI.Domain.Modules.Reviews.Entities;
using SkillSwapAPI.Domain.Modules.SwapRequests.Entities;
using SkillSwapAPI.Infrastructure.Identity;

namespace SkillSwapAPI.Infrastructure.Persistence.Data.Configurations.Reviews;

public sealed class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("Reviews");
        builder.HasKey(review => review.Id);
        builder.Property(review => review.Rating).IsRequired();
        builder.Property(review => review.Comment).HasMaxLength(1000);
        builder.HasIndex(review => new { review.SwapRequestId, review.ReviewerId }).IsUnique();
        builder.HasIndex(review => review.RevieweeId);
        builder.HasOne<SwapRequest>()
            .WithMany()
            .HasForeignKey(review => review.SwapRequestId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(review => review.ReviewerId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(review => review.RevieweeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
