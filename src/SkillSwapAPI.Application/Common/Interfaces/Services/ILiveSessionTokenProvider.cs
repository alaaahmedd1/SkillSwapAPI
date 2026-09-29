namespace SkillSwapAPI.Application.Common.Interfaces.Services;

public interface ILiveSessionTokenProvider
{
    string GenerateRoomToken();
}
