using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkillSwapAPI.Infrastructure.Settings;

public sealed class IdentitySettings
{
    public const string SectionName = "Identity";

    public PasswordSettings Password { get; init; } = new();
    public UserSettings User { get; init; } = new();
}

public sealed class PasswordSettings
{
    public bool RequireDigit { get; init; }
    public bool RequireLowercase { get; init; }
    public bool RequireNonAlphanumeric { get; init; }
    public bool RequireUppercase { get; init; }
    public int RequiredLength { get; init; }
}

public sealed class UserSettings
{
    public bool RequireUniqueEmail { get; init; }
}
