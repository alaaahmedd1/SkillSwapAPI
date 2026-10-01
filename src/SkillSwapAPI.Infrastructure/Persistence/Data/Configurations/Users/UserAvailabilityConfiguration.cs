using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkillSwapAPI.Domain.Modules.Users.Entities;
using SkillSwapAPI.Infrastructure.Identity;

namespace SkillSwapAPI.Infrastructure.Persistence.Data.Configurations.Users;

public sealed class UserAvailabilityConfiguration : IEntityTypeConfiguration<UserAvailability>
{
    public void Configure(EntityTypeBuilder<UserAvailability> builder)
    {
        builder.ToTable("UserAvailability");
        builder.HasKey(userAvailability => userAvailability.Id);
        builder.HasIndex(userAvailability => new
        {
            userAvailability.UserId,
            userAvailability.DayOfWeek,
            userAvailability.TimeBlock
        }).IsUnique();
        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(userAvailability => userAvailability.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
