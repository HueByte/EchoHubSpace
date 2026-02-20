using EchoHub.App.Hubs;
using EchoHub.Core.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EchoHub.App.Services;

/// <summary>
/// Background service that periodically checks for stale or unresponsive servers.
/// Sends alive-check pings to stale servers, marks unresponsive ones as offline,
/// and removes offline servers that have exceeded the cleanup threshold.
/// </summary>
public class InactiveServerCleanupService(
    IServiceScopeFactory scopeFactory,
    IHubContext<ServerHub> hubContext,
    ILogger<InactiveServerCleanupService> logger) : BackgroundService
{
    private static readonly TimeSpan CleanupInterval = TimeSpan.FromMinutes(1);

    // Online server hasn't reported in 5 min — send an alive check
    private static readonly TimeSpan StaleThreshold = TimeSpan.FromMinutes(5);

    // Online server didn't respond to alive checks within 7 min — mark offline
    private static readonly TimeSpan UnresponsiveThreshold = TimeSpan.FromMinutes(7);

    // Offline servers get removed from DB after 5 min
    private static readonly TimeSpan OfflineCleanupThreshold = TimeSpan.FromMinutes(5);

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(CleanupInterval, stoppingToken);

            try
            {
                using var scope = scopeFactory.CreateScope();
                var repo = scope.ServiceProvider.GetRequiredService<IServerRepository>();
                var serverService = scope.ServiceProvider.GetRequiredService<IServerService>();

                // 1. Remove offline servers that have been stale long enough
                await repo.RemoveInactiveAsync(OfflineCleanupThreshold);

                // 2. Check stale online servers — ping or mark offline
                var staleServers = await repo.GetStaleOnlineAsync(StaleThreshold);

                foreach (var server in staleServers)
                {
                    var staleDuration = DateTime.UtcNow - server.LastSeenAt;
                    var connectionIds = ServerHub.GetConnectionIdsForHost(server.Host).ToList();

                    if (staleDuration > UnresponsiveThreshold || connectionIds.Count == 0)
                    {
                        // Server didn't respond to pings, or has no tracked connections — mark offline
                        logger.LogWarning(
                            "Server {Host} unresponsive (stale for {Duration}, {Connections} connections) — marking offline",
                            server.Host, staleDuration, connectionIds.Count);

                        await serverService.SetServerOfflineAsync(server.Host);
                        await hubContext.Clients.Group("web-clients")
                            .SendAsync("ServerOffline", new { server.Host }, stoppingToken);
                    }
                    else
                    {
                        // Send alive check — client should respond with Heartbeat()
                        logger.LogDebug("Sending alive check to {Host} ({Count} connections)",
                            server.Host, connectionIds.Count);

                        foreach (var connectionId in connectionIds)
                        {
                            await hubContext.Clients.Client(connectionId)
                                .SendAsync("Ping", stoppingToken);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during server health check");
            }
        }
    }
}
