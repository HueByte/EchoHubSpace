using System.Collections.Concurrent;
using EchoHub.App.DTOs;
using EchoHub.App.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace EchoHub.Infrastructure.Hubs;

public class ServerHub(IServiceScopeFactory scopeFactory, ILogger<ServerHub> logger) : Hub
{
    // Maps connectionId -> (host, port) so we can mark servers offline on disconnect
    private static readonly ConcurrentDictionary<string, (string Host, int Port)> ConnectedServers = new();

    /// <summary>
    /// Called by an EchoHub server to register/update itself on the server list.
    /// </summary>
    public async Task RegisterServer(RegisterServerDto dto)
    {
        using var scope = scopeFactory.CreateScope();
        var serverService = scope.ServiceProvider.GetRequiredService<IServerService>();

        var server = await serverService.RegisterServerAsync(dto);

        ConnectedServers[Context.ConnectionId] = (dto.Host, dto.Port);

        logger.LogInformation("Server registered: {Name} at {Host}:{Port} (connection {ConnectionId})",
            dto.Name, dto.Host, dto.Port, Context.ConnectionId);

        await Clients.Group("web-clients").SendAsync("ServerUpdated", server);
    }

    /// <summary>
    /// Called by an EchoHub server to update its user count.
    /// </summary>
    public async Task UpdateUserCount(int userCount)
    {
        if (!ConnectedServers.TryGetValue(Context.ConnectionId, out var info))
            return;

        using var scope = scopeFactory.CreateScope();
        var serverService = scope.ServiceProvider.GetRequiredService<IServerService>();

        var dto = new RegisterServerDto("", null, info.Host, info.Port, userCount);
        var server = await serverService.RegisterServerAsync(dto);

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
        if (ConnectedServers.TryRemove(Context.ConnectionId, out var info))
        {
            logger.LogInformation("Server disconnected: {Host}:{Port} (connection {ConnectionId})",
                info.Host, info.Port, Context.ConnectionId);

            using var scope = scopeFactory.CreateScope();
            var serverService = scope.ServiceProvider.GetRequiredService<IServerService>();

            await serverService.SetServerOfflineAsync(info.Host, info.Port);

            await Clients.Group("web-clients").SendAsync("ServerOffline", new { info.Host, info.Port });
        }

        await base.OnDisconnectedAsync(exception);
    }
}
