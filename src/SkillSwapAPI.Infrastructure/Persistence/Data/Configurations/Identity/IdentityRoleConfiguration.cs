using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SkillSwapAPI.Infrastructure.Persistence.Data.Configurations.Identity;

public sealed class IdentityRoleConfiguration : IEntityTypeConfiguration<IdentityRole<Guid>>
{
    public void Configure(EntityTypeBuilder<IdentityRole<Guid>> builder)
    {
        builder.HasData(
            new IdentityRole<Guid>
            {
                Id = Guid.Parse("a0000000-0000-0000-0000-000000000001"),
                Name = "Admin",
                NormalizedName = "ADMIN",
                ConcurrencyStamp = "a0000000-0000-0000-0000-000000000011",
            },
            new IdentityRole<Guid>
            {
                Id = Guid.Parse("a0000000-0000-0000-0000-000000000002"),
                Name = "User",
                NormalizedName = "USER",
                ConcurrencyStamp = "a0000000-0000-0000-0000-000000000012",
            });
    }
}
