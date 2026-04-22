using System.Collections.Concurrent;
using EchoHub.Core.DTOs;
using EchoHub.Core.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace EchoHub.App.Hubs;

/// <summary>
/// SignalR hub that manages real-time communication between EchoHub server instances and web clients.
/// Server instances register, send heartbeats, and update user counts through this hub.
/// Web clients join a broadcast group to receive live server status updates.
/// </summary>
public class ServerHub(IServiceScopeFactory scopeFactory, ILogger<ServerHub> logger) : Hub
{
    // Maps connectionId -> server Id for reverse lookup on disconnect
    private static readonly ConcurrentDictionary<string, Guid> ConnectionToServer = new();

    // Tracks active connection count per server so we only go offline when all connections drop
    private static readonly ConcurrentDictionary<Guid, int> ServerConnectionCount = new();

    private static readonly object Lock = new();

    /// <summary>
    /// Called by an EchoHub server to register/update itself on the server list.
    /// </summary>
    public async Task RegisterServer(RegisterServerDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name) || dto.Hosts is null || dto.Hosts.Length == 0)
            return;

        using var scope = scopeFactory.CreateScope();
        var serverService = scope.ServiceProvider.GetRequiredService<IServerService>();

        var server = await serverService.RegisterServerAsync(dto);

        lock (Lock)
        {
            // If this connection was previously mapped to a different server, decrement the old server's count
            if (ConnectionToServer.TryGetValue(Context.ConnectionId, out var previousId) && previousId != server.Id)
            {
                var remaining = ServerConnectionCount.AddOrUpdate(previousId, 0, (_, count) => count - 1);
                if (remaining <= 0)
                    ServerConnectionCount.TryRemove(previousId, out _);
            }

            ConnectionToServer[Context.ConnectionId] = server.Id;
            ServerConnectionCount.AddOrUpdate(server.Id, 1, (_, count) => count + 1);
        }

        logger.LogInformation("Server registered: {Name} [{Id}] (connection {ConnectionId})",
            dto.Name, server.Id, Context.ConnectionId);

        await Clients.Group("web-clients").SendAsync("ServerUpdated", server);
    }

    /// <summary>
    /// Called by an EchoHub server to update its user count.
    /// </summary>
    public async Task UpdateUserCount(int userCount)
    {
        if (!ConnectionToServer.TryGetValue(Context.ConnectionId, out var serverId))
            return;

        using var scope = scopeFactory.CreateScope();
        var serverService = scope.ServiceProvider.GetRequiredService<IServerService>();

        var server = await serverService.UpdateUserCountAsync(serverId, userCount);
        if (server is not null)
            await Clients.Group("web-clients").SendAsync("ServerUpdated", server);
    }

    /// <summary>
    /// Called by an EchoHub server in response to an alive check (Ping).
    /// Refreshes LastSeenAt to confirm the server is still responsive.
    /// </summary>
    public async Task Heartbeat()
    {
        if (!ConnectionToServer.TryGetValue(Context.ConnectionId, out var serverId))
            return;

        using var scope = scopeFactory.CreateScope();
        var serverService = scope.ServiceProvider.GetRequiredService<IServerService>();
        await serverService.RefreshLastSeenAsync(serverId);

        logger.LogDebug("Heartbeat received from server {Id} (connection {ConnectionId})", serverId, Context.ConnectionId);
    }

    /// <summary>
    /// Returns all tracked connection IDs for a given server.
    /// Used by the cleanup service to send alive checks.
    /// </summary>
    /// <param name="serverId">The server ID to look up connections for.</param>
    public static IEnumerable<string> GetConnectionIdsForServer(Guid serverId)
    {
        return ConnectionToServer
            .Where(kvp => kvp.Value == serverId)
            .Select(kvp => kvp.Key);
    }

    /// <summary>
    /// Called by web clients to join the broadcast group for real-time updates.
    /// </summary>
    public async Task JoinWebClients()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "web-clients");
    }

    /// <summary>
    /// Handles client disconnection by decrementing the connection count for the associated server.
    /// When all connections for a server are dropped, the server is marked offline and web clients are notified.
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (ConnectionToServer.TryRemove(Context.ConnectionId, out var serverId))
        {
            bool shouldGoOffline;
            lock (Lock)
            {
                var remaining = ServerConnectionCount.AddOrUpdate(serverId, 0, (_, count) => count - 1);
                shouldGoOffline = remaining <= 0;
                if (shouldGoOffline)
                    ServerConnectionCount.TryRemove(serverId, out _);
            }

            if (shouldGoOffline)
            {
                logger.LogInformation("Server offline: {Id} (last connection {ConnectionId} dropped)",
                    serverId, Context.ConnectionId);

                using var scope = scopeFactory.CreateScope();
                var serverService = scope.ServiceProvider.GetRequiredService<IServerService>();

                await serverService.SetServerOfflineAsync(serverId);
                await Clients.Group("web-clients").SendAsync("ServerOffline", new { Id = serverId });
            }
            else
            {
                logger.LogDebug("Connection {ConnectionId} dropped for server {Id}, other connections still active",
                    Context.ConnectionId, serverId);
            }
        }

        await base.OnDisconnectedAsync(exception);
    }
}
