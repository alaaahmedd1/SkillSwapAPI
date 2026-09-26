using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkillSwapAPI.Domain.Modules.Users.Entities;
using SkillSwapAPI.Infrastructure.Identity;

namespace SkillSwapAPI.Infrastructure.Persistence.Data.Configurations.Users;

public sealed class UserSkillConfiguration : IEntityTypeConfiguration<UserSkill>
{
    public void Configure(EntityTypeBuilder<UserSkill> builder)
    {
        builder.ToTable("UserSkills");
        builder.HasKey(userSkill => userSkill.Id);
        builder.Property(userSkill => userSkill.Type).IsRequired();
        builder.Property(userSkill => userSkill.ProficiencyLevel).IsRequired();
        builder.HasIndex(userSkill => new { userSkill.UserId, userSkill.SkillId, userSkill.Type }).IsUnique();
        builder.HasOne(userSkill => userSkill.Skill)
            .WithMany()
            .HasForeignKey(userSkill => userSkill.SkillId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(userSkill => userSkill.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
