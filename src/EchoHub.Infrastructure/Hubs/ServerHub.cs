using System.Collections.Concurrent;
using EchoHub.App.DTOs;
using EchoHub.App.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace EchoHub.Infrastructure.Hubs;

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

        ConnectionToHost[Context.ConnectionId] = dto.Host;
        lock (Lock)
        {
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
    /// Called by web clients to join the broadcast group for real-time updates.
    /// </summary>
    public async Task JoinWebClients()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "web-clients");
    }

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
