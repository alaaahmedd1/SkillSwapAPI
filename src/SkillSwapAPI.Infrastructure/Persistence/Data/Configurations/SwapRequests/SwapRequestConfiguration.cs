using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkillSwapAPI.Domain.Modules.SwapRequests.Entities;
using SkillSwapAPI.Infrastructure.Identity;

namespace SkillSwapAPI.Infrastructure.Persistence.Data.Configurations.SwapRequests;

public sealed class SwapRequestConfiguration : IEntityTypeConfiguration<SwapRequest>
{
    public void Configure(EntityTypeBuilder<SwapRequest> builder)
    {
        builder.ToTable("SwapRequests");
        builder.HasKey(swapRequest => swapRequest.Id);
        builder.Property(swapRequest => swapRequest.Status).IsRequired();
        builder.Property(swapRequest => swapRequest.ProposedScheduleDetails).HasMaxLength(1000);
        builder.Property(swapRequest => swapRequest.RowVersion).IsRowVersion();
        builder.HasIndex(swapRequest => new
        {
            swapRequest.RequesterId,
            swapRequest.ReceiverId,
            swapRequest.OfferedSkillId,
            swapRequest.RequestedSkillId
        })
            .IsUnique()
            .HasFilter("[Status] = 1");
        builder.HasOne(swapRequest => swapRequest.OfferedSkill)
            .WithMany()
            .HasForeignKey(swapRequest => swapRequest.OfferedSkillId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(swapRequest => swapRequest.RequestedSkill)
            .WithMany()
            .HasForeignKey(swapRequest => swapRequest.RequestedSkillId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(swapRequest => swapRequest.RequesterId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(swapRequest => swapRequest.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
