using EchoHub.Core.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EchoHub.Infrastructure.Services;

public class InactiveServerCleanupService(
    IServiceScopeFactory scopeFactory,
    ILogger<InactiveServerCleanupService> logger) : BackgroundService
{
    private static readonly TimeSpan CleanupInterval = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan OfflineThreshold = TimeSpan.FromMinutes(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(CleanupInterval, stoppingToken);

            try
            {
                using var scope = scopeFactory.CreateScope();
                var repo = scope.ServiceProvider.GetRequiredService<IServerRepository>();
                await repo.RemoveInactiveAsync(OfflineThreshold);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error cleaning up inactive servers");
            }
        }
    }
}
