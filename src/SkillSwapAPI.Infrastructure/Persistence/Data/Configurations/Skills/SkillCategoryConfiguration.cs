using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkillSwapAPI.Domain.Skills.Entities;

namespace SkillSwapAPI.Infrastructure.Persistence.Data.Configurations.Skills;

public sealed class SkillCategoryConfiguration : IEntityTypeConfiguration<SkillCategory>
{
    public void Configure(EntityTypeBuilder<SkillCategory> builder)
    {
        builder.ToTable("SkillCategories");
        builder.HasKey(category => category.Id);
        builder.Property(category => category.Name).IsRequired().HasMaxLength(100);
        builder.Property(category => category.Description).HasMaxLength(500);
        builder.Property(category => category.IsActive).HasDefaultValue(true);
        builder.HasIndex(category => category.Name).IsUnique();

        builder.HasData(
            new SkillCategory { Id = 1, Name = "Programming Languages", Description = "General-purpose and specialized programming languages", IsActive = true },
            new SkillCategory { Id = 2, Name = "Web Development", Description = "Frontend and backend web frameworks", IsActive = true },
            new SkillCategory { Id = 3, Name = "Mobile Development", Description = "Native and cross-platform mobile app development", IsActive = true },
            new SkillCategory { Id = 4, Name = "Design", Description = "UI/UX and graphic design", IsActive = true },
            new SkillCategory { Id = 5, Name = "Languages", Description = "Spoken/written human languages", IsActive = true },
            new SkillCategory { Id = 6, Name = "Data & AI", Description = "Data science, machine learning, and analytics", IsActive = true },
            new SkillCategory { Id = 7, Name = "Business & Marketing", Description = "Business, marketing, and soft skills", IsActive = true });
    }
}
