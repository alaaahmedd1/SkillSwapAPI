using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkillSwapAPI.Domain.Modules.SessionProposals.Entities;
using SkillSwapAPI.Domain.Modules.SwapRequests.Entities;
using SkillSwapAPI.Infrastructure.Identity;

namespace SkillSwapAPI.Infrastructure.Persistence.Data.Configurations.SessionProposals;

public sealed class SessionProposalConfiguration : IEntityTypeConfiguration<SessionProposal>
{
    public void Configure(EntityTypeBuilder<SessionProposal> builder)
    {
        builder.ToTable("SessionProposals");
        builder.HasKey(proposal => proposal.Id);
        builder.Property(proposal => proposal.ScheduledDate).IsRequired();
        builder.Property(proposal => proposal.StartTime).IsRequired();
        builder.Property(proposal => proposal.EndTime).IsRequired();
        builder.Property(proposal => proposal.DurationMinutes).IsRequired();
        builder.Property(proposal => proposal.Status).HasConversion<int>().IsRequired();
        builder.Property(proposal => proposal.CreatedAtUtc).IsRequired();
        builder.HasIndex(proposal => proposal.SwapRequestId);
        builder.HasOne<SwapRequest>()
            .WithMany()
            .HasForeignKey(proposal => proposal.SwapRequestId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<AppUser>()
            .WithMany()
            .HasForeignKey(proposal => proposal.ProposerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
