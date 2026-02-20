using EchoHub.Core.Entities;
using EchoHub.Core.Interfaces;
using EchoHub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EchoHub.Infrastructure.Repositories;

/// <summary>
/// Entity Framework Core implementation of <see cref="IServerRepository"/> using <see cref="AppDbContext"/>.
/// </summary>
public class ServerRepository(AppDbContext context) : IServerRepository
{
    /// <inheritdoc />
    public async Task<IEnumerable<Server>> GetAllAsync()
    {
        return await context.Servers
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<Server?> GetByIdAsync(Guid id)
    {
        return await context.Servers.FindAsync(id);
    }

    /// <inheritdoc />
    public async Task<Server?> GetByHostAsync(string host)
    {
        return await context.Servers
            .FirstOrDefaultAsync(s => s.Host == host);
    }

    /// <inheritdoc />
    public async Task<Server> AddAsync(Server server)
    {
        context.Servers.Add(server);
        await context.SaveChangesAsync();
        return server;
    }

    /// <inheritdoc />
    public async Task UpdateAsync(Server server)
    {
        context.Servers.Update(server);
        await context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(Guid id)
    {
        var server = await context.Servers.FindAsync(id);
        if (server is null) return false;

        context.Servers.Remove(server);
        await context.SaveChangesAsync();
        return true;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Server>> GetStaleOnlineAsync(TimeSpan threshold)
    {
        var cutoff = DateTime.UtcNow - threshold;
        return await context.Servers
            .Where(s => s.IsOnline && s.LastSeenAt < cutoff)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task RemoveInactiveAsync(TimeSpan offlineThreshold)
    {
        var cutoff = DateTime.UtcNow - offlineThreshold;
        var inactive = await context.Servers
            .Where(s => !s.IsOnline && s.LastSeenAt < cutoff)
            .ToListAsync();

        if (inactive.Count > 0)
        {
            context.Servers.RemoveRange(inactive);
            await context.SaveChangesAsync();
        }
    }
}
