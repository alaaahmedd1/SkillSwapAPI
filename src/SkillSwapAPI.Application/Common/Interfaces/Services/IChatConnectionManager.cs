namespace SkillSwapAPI.Application.Common.Interfaces.Services;

public interface IChatConnectionManager
{
    Task DisconnectUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
