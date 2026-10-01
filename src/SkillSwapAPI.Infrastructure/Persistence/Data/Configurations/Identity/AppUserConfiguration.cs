using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkillSwapAPI.Infrastructure.Identity;

namespace SkillSwapAPI.Infrastructure.Persistence.Data.Configurations.Identity;

public sealed class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.Property(u => u.FirstName).HasMaxLength(50).IsRequired();
        builder.Property(u => u.LastName).HasMaxLength(50).IsRequired();
        builder.Property(u => u.AverageRating).HasColumnType("decimal(3,2)").HasDefaultValue(0.00m);
        builder.Property(u => u.TotalReviewsCount).HasDefaultValue(0);
        builder.Property(u => u.IsActive).HasDefaultValue(true);
        builder.Property(u => u.CreatedAtUtc).IsRequired();
        builder.Property(u => u.Title).HasMaxLength(100);
        builder.Property(u => u.Bio).HasMaxLength(200);
        builder.Property(u => u.City).HasMaxLength(60);
        builder.Property(u => u.Country).HasMaxLength(60);
        builder.Property(u => u.TimeZone).HasMaxLength(64);
        builder.Property(u => u.OpenForInstantSwaps).HasDefaultValue(true);
    }
}
