using System.Text.Json.Serialization;

namespace SkillSwapAPI.Domain.Identity;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SocialProvider
{
    Google,
    Facebook,
    Apple
}
