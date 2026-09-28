using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkillSwapAPI.Domain.Modules.Chat.Entities;
using SkillSwapAPI.Domain.Modules.SwapRequests.Entities;

namespace SkillSwapAPI.Infrastructure.Persistence.Data.Configurations.Chat;

public sealed class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.ToTable("Conversations");
        builder.HasKey(conversation => conversation.Id);
        builder.HasIndex(conversation => conversation.SwapRequestId).IsUnique();
        builder.HasOne<SwapRequest>()
            .WithMany()
            .HasForeignKey(conversation => conversation.SwapRequestId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
