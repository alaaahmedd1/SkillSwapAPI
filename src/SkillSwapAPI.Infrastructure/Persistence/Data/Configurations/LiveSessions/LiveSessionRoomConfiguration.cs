using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkillSwapAPI.Domain.Modules.LiveSessions.Entities;
using SkillSwapAPI.Domain.Modules.SwapRequests.Entities;

namespace SkillSwapAPI.Infrastructure.Persistence.Data.Configurations.LiveSessions;

public sealed class LiveSessionRoomConfiguration : IEntityTypeConfiguration<LiveSessionRoom>
{
    public void Configure(EntityTypeBuilder<LiveSessionRoom> builder)
    {
        builder.ToTable("LiveSessionRooms");
        builder.HasKey(room => room.Id);
        builder.Property(room => room.RoomToken).IsRequired().HasMaxLength(128);
        builder.Property(room => room.ScheduledStartTime).IsRequired();
        builder.Property(room => room.DurationSeconds).IsRequired();
        builder.Property(room => room.Status).HasConversion<int>().IsRequired();
        builder.HasIndex(room => room.SwapRequestId).IsUnique();
        builder.HasOne<SwapRequest>()
            .WithMany()
            .HasForeignKey(room => room.SwapRequestId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
