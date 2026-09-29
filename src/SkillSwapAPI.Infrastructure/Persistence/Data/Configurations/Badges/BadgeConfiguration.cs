using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkillSwapAPI.Domain.Modules.Badges.Entities;

namespace SkillSwapAPI.Infrastructure.Persistence.Data.Configurations.Badges;

public sealed class BadgeConfiguration : IEntityTypeConfiguration<Badge>
{
    public void Configure(EntityTypeBuilder<Badge> builder)
    {
        builder.ToTable("Badges");
        builder.HasKey(badge => badge.Id);
        builder.Property(badge => badge.Name).HasMaxLength(50).IsRequired();
        builder.HasIndex(badge => badge.Name).IsUnique();
        builder.Property(badge => badge.Description).HasMaxLength(250).IsRequired();
        builder.Property(badge => badge.IconUrl).HasMaxLength(500).IsRequired();
        builder.Property(badge => badge.IsActive).HasDefaultValue(true).IsRequired();

        builder.HasData(
            new Badge
            {
                Id = 1,
                Name = "Super Patient",
                Description = "Awarded to mentors who explain difficult concepts clearly.",
                IconUrl = "/images/badges/super-patient.svg",
                IsActive = true
            },
            new Badge
            {
                Id = 2,
                Name = "Best Tutor",
                Description = "Awarded for outstanding teaching and guidance in sessions.",
                IconUrl = "/images/badges/best-tutor.svg",
                IsActive = true
            },
            new Badge
            {
                Id = 3,
                Name = "Problem Solver",
                Description = "Awarded for breaking down tough problems into simple steps.",
                IconUrl = "/images/badges/problem-solver.svg",
                IsActive = true
            },
            new Badge
            {
                Id = 4,
                Name = "Great Communicator",
                Description = "Awarded for clear, responsive, and helpful communication.",
                IconUrl = "/images/badges/great-communicator.svg",
                IsActive = true
            },
            new Badge
            {
                Id = 5,
                Name = "Reliable Partner",
                Description = "Awarded for consistency and punctuality in scheduled sessions.",
                IconUrl = "/images/badges/reliable-partner.svg",
                IsActive = true
            });
    }
}
