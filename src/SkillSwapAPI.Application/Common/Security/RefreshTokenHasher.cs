using System.Security.Cryptography;
using System.Text;

namespace SkillSwapAPI.Application.Common.Security;

public static class RefreshTokenHasher
{
    public static string Hash(string refreshToken) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));
}
