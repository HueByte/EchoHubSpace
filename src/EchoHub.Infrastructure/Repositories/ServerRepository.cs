using EchoHub.Core.Entities;
using EchoHub.Core.Interfaces;
using EchoHub.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EchoHub.Infrastructure.Repositories;

public class ServerRepository(AppDbContext context) : IServerRepository
{
    public async Task<IEnumerable<Server>> GetAllAsync()
    {
        return await context.Servers
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<Server?> GetByIdAsync(Guid id)
    {
        return await context.Servers.FindAsync(id);
    }

    public async Task<Server?> GetByHostAsync(string host)
    {
        return await context.Servers
            .FirstOrDefaultAsync(s => s.Host == host);
    }

    public async Task<Server> AddAsync(Server server)
    {
        context.Servers.Add(server);
        await context.SaveChangesAsync();
        return server;
    }

    public async Task UpdateAsync(Server server)
    {
        context.Servers.Update(server);
        await context.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var server = await context.Servers.FindAsync(id);
        if (server is null) return false;

        context.Servers.Remove(server);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task RemoveInactiveAsync(TimeSpan offlineThreshold)
    {
        var cutoff = DateTime.UtcNow - offlineThreshold;
        var inactive = await context.Servers
            .Where(s => s.LastSeenAt < cutoff)
            .ToListAsync();

        if (inactive.Count > 0)
        {
            context.Servers.RemoveRange(inactive);
            await context.SaveChangesAsync();
        }
    }
}
