using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkillSwapAPI.Domain.Modules.Administration.Entities;
using SkillSwapAPI.Infrastructure.Identity;

namespace SkillSwapAPI.Infrastructure.Persistence.Data.Configurations.Administration;

public sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        builder.HasKey(auditLog => auditLog.Id);
        builder.Property(auditLog => auditLog.Action).HasMaxLength(100).IsRequired();
        builder.Property(auditLog => auditLog.TargetEntity).HasMaxLength(100).IsRequired();
        builder.Property(auditLog => auditLog.TargetEntityId).HasMaxLength(100);
        builder.Property(auditLog => auditLog.Reason).HasMaxLength(500);
        builder.Property(auditLog => auditLog.PerformedAtUtc).IsRequired();
        builder.HasIndex(auditLog => auditLog.AdminId);
        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(auditLog => auditLog.AdminId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
