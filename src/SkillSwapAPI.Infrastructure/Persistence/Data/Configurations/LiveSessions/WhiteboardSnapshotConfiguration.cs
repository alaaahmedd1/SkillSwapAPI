using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkillSwapAPI.Domain.Modules.LiveSessions.Entities;

namespace SkillSwapAPI.Infrastructure.Persistence.Data.Configurations.LiveSessions
{
    public class WhiteboardSnapshotConfiguration : IEntityTypeConfiguration<WhiteboardSnapshot>
    {
        public void Configure(EntityTypeBuilder<WhiteboardSnapshot> builder)
        {
            builder.ToTable("WhiteboardSnapshots");

            builder.HasKey(snapshot => snapshot.Id);

            builder.Property(snapshot => snapshot.CanvasDataJson)
                .IsRequired();

            builder.Property(snapshot => snapshot.UpdatedAtUtc)
                .IsRequired();

            builder.HasIndex(snapshot => snapshot.RoomId)
                .IsUnique();

            builder.HasOne<LiveSessionRoom>()
                .WithMany()
                .HasForeignKey(snapshot => snapshot.RoomId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
