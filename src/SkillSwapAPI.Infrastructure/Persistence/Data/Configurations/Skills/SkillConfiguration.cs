using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkillSwapAPI.Domain.Skills.Entities;

namespace SkillSwapAPI.Infrastructure.Persistence.Data.Configurations.Skills;

public sealed class SkillConfiguration : IEntityTypeConfiguration<Skill>
{
    public void Configure(EntityTypeBuilder<Skill> builder)
    {
        builder.ToTable("Skills");
        builder.HasKey(skill => skill.Id);
        builder.Property(skill => skill.Name).IsRequired().HasMaxLength(100);
        builder.Property(skill => skill.Description).HasMaxLength(500);
        builder.HasIndex(skill => skill.Name).IsUnique();
        builder.HasOne(skill => skill.Category)
            .WithMany(category => category.Skills)
            .HasForeignKey(skill => skill.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(
            new Skill { Id = Guid.Parse("11111111-0000-0000-0000-000000000001"), CategoryId = 1, Name = "C#", Description = "The C# programming language" },
            new Skill { Id = Guid.Parse("11111111-0000-0000-0000-000000000002"), CategoryId = 1, Name = "Python", Description = "The Python programming language" },
            new Skill { Id = Guid.Parse("11111111-0000-0000-0000-000000000003"), CategoryId = 1, Name = "Java", Description = "The Java programming language" },
            new Skill { Id = Guid.Parse("22222222-0000-0000-0000-000000000001"), CategoryId = 2, Name = "Angular", Description = "Angular framework" },
            new Skill { Id = Guid.Parse("22222222-0000-0000-0000-000000000002"), CategoryId = 2, Name = "React", Description = "React library" },
            new Skill { Id = Guid.Parse("22222222-0000-0000-0000-000000000003"), CategoryId = 2, Name = "ASP.NET Core", Description = "ASP.NET Core web framework" },
            new Skill { Id = Guid.Parse("33333333-0000-0000-0000-000000000001"), CategoryId = 3, Name = "Flutter", Description = "Cross-platform mobile framework" },
            new Skill { Id = Guid.Parse("33333333-0000-0000-0000-000000000002"), CategoryId = 3, Name = "Swift", Description = "iOS native development" },
            new Skill { Id = Guid.Parse("44444444-0000-0000-0000-000000000001"), CategoryId = 4, Name = "UI/UX Design", Description = "User interface and experience design" },
            new Skill { Id = Guid.Parse("44444444-0000-0000-0000-000000000002"), CategoryId = 4, Name = "Figma", Description = "Figma design tool" },
            new Skill { Id = Guid.Parse("55555555-0000-0000-0000-000000000001"), CategoryId = 5, Name = "English", Description = "English language" },
            new Skill { Id = Guid.Parse("55555555-0000-0000-0000-000000000002"), CategoryId = 5, Name = "German", Description = "German language" },
            new Skill { Id = Guid.Parse("66666666-0000-0000-0000-000000000001"), CategoryId = 6, Name = "Machine Learning", Description = "Machine learning fundamentals" },
            new Skill { Id = Guid.Parse("66666666-0000-0000-0000-000000000002"), CategoryId = 6, Name = "Data Analysis", Description = "Data analysis and visualization" },
            new Skill { Id = Guid.Parse("77777777-0000-0000-0000-000000000001"), CategoryId = 7, Name = "Digital Marketing", Description = "SEO, ads, and social media marketing" });
    }
}
