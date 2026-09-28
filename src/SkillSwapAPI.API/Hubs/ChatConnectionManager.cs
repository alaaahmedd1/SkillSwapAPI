using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;
using SkillSwapAPI.Application.Common.Interfaces.Services;

namespace SkillSwapAPI.API.Hubs;

public sealed class ChatConnectionManager : IChatConnectionManager
{
    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<string, HubCallerContext>> _connections = new();

    public void AddConnection(Guid userId, HubCallerContext context)
    {
        var userConnections = _connections.GetOrAdd(
            userId, _ => new ConcurrentDictionary<string, HubCallerContext>());

        userConnections[context.ConnectionId] = context;
    }

    public void RemoveConnection(Guid userId, string connectionId)
    {
        if (!_connections.TryGetValue(userId, out var userConnections))
        {
            return;
        }

        userConnections.TryRemove(connectionId, out _);

        if (userConnections.IsEmpty)
        {
            _connections.TryRemove(userId, out _);
        }
    }

    public Task DisconnectUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        if (_connections.TryRemove(userId, out var userConnections))
        {
            foreach (var connection in userConnections.Values)
            {
                connection.Abort();
            }
        }

        return Task.CompletedTask;
    }
}
