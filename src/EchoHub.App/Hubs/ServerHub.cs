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
    // Maps connectionId -> host for reverse lookup on disconnect
    private static readonly ConcurrentDictionary<string, string> ConnectionToHost = new();

    // Tracks active connection count per host so we only go offline when all connections drop
    private static readonly ConcurrentDictionary<string, int> HostConnectionCount = new();

    private static readonly object Lock = new();

    /// <summary>
    /// Called by an EchoHub server to register/update itself on the server list.
    /// </summary>
    public async Task RegisterServer(RegisterServerDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Host) || string.IsNullOrWhiteSpace(dto.Name))
            return;

        using var scope = scopeFactory.CreateScope();
        var serverService = scope.ServiceProvider.GetRequiredService<IServerService>();

        var server = await serverService.RegisterServerAsync(dto);

        lock (Lock)
        {
            // If this connection was previously mapped to a different host, decrement the old host's count
            if (ConnectionToHost.TryGetValue(Context.ConnectionId, out var previousHost) && previousHost != dto.Host)
            {
                var remaining = HostConnectionCount.AddOrUpdate(previousHost, 0, (_, count) => count - 1);
                if (remaining <= 0)
                    HostConnectionCount.TryRemove(previousHost, out _);
            }

            ConnectionToHost[Context.ConnectionId] = dto.Host;
            HostConnectionCount.AddOrUpdate(dto.Host, 1, (_, count) => count + 1);
        }

        logger.LogInformation("Server registered: {Name} at {Host} (connection {ConnectionId})",
            dto.Name, dto.Host, Context.ConnectionId);

        await Clients.Group("web-clients").SendAsync("ServerUpdated", server);
    }

    /// <summary>
    /// Called by an EchoHub server to update its user count.
    /// </summary>
    public async Task UpdateUserCount(int userCount)
    {
        if (!ConnectionToHost.TryGetValue(Context.ConnectionId, out var host))
            return;

        using var scope = scopeFactory.CreateScope();
        var serverService = scope.ServiceProvider.GetRequiredService<IServerService>();

        var server = await serverService.UpdateUserCountAsync(host, userCount);
        if (server is not null)
            await Clients.Group("web-clients").SendAsync("ServerUpdated", server);
    }

    /// <summary>
    /// Called by an EchoHub server in response to an alive check (Ping).
    /// Refreshes LastSeenAt to confirm the server is still responsive.
    /// </summary>
    public async Task Heartbeat()
    {
        if (!ConnectionToHost.TryGetValue(Context.ConnectionId, out var host))
            return;

        using var scope = scopeFactory.CreateScope();
        var serverService = scope.ServiceProvider.GetRequiredService<IServerService>();
        await serverService.RefreshLastSeenAsync(host);

        logger.LogDebug("Heartbeat received from {Host} (connection {ConnectionId})", host, Context.ConnectionId);
    }

    /// <summary>
    /// Returns all tracked connection IDs for a given host.
    /// Used by the cleanup service to send alive checks.
    /// </summary>
    /// <param name="host">The host address to look up connections for.</param>
    public static IEnumerable<string> GetConnectionIdsForHost(string host)
    {
        return ConnectionToHost
            .Where(kvp => kvp.Value == host)
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
    /// Handles client disconnection by decrementing the connection count for the associated host.
    /// When all connections for a host are dropped, the server is marked offline and web clients are notified.
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (ConnectionToHost.TryRemove(Context.ConnectionId, out var host))
        {
            bool shouldGoOffline;
            lock (Lock)
            {
                var remaining = HostConnectionCount.AddOrUpdate(host, 0, (_, count) => count - 1);
                shouldGoOffline = remaining <= 0;
                if (shouldGoOffline)
                    HostConnectionCount.TryRemove(host, out _);
            }

            if (shouldGoOffline)
            {
                logger.LogInformation("Server offline: {Host} (last connection {ConnectionId} dropped)",
                    host, Context.ConnectionId);

                using var scope = scopeFactory.CreateScope();
                var serverService = scope.ServiceProvider.GetRequiredService<IServerService>();

                await serverService.SetServerOfflineAsync(host);
                await Clients.Group("web-clients").SendAsync("ServerOffline", new { Host = host });
            }
            else
            {
                logger.LogDebug("Connection {ConnectionId} dropped for {Host}, other connections still active",
                    Context.ConnectionId, host);
            }
        }

        await base.OnDisconnectedAsync(exception);
    }
}
