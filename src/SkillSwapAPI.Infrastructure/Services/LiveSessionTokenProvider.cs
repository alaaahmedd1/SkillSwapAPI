using System.Security.Cryptography;
using SkillSwapAPI.Application.Common.Interfaces.Services;

namespace SkillSwapAPI.Infrastructure.Services;

public sealed class LiveSessionTokenProvider : ILiveSessionTokenProvider
{
    public string GenerateRoomToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    }
}
