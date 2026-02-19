using EchoHub.Core.Entities;

namespace EchoHub.Core.Interfaces;

public interface IServerRepository
{
    Task<IEnumerable<Server>> GetAllAsync();
    Task<Server?> GetByIdAsync(Guid id);
    Task<Server?> GetByHostAsync(string host);
    Task<Server> AddAsync(Server server);
    Task UpdateAsync(Server server);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<Server>> GetStaleOnlineAsync(TimeSpan threshold);
    Task RemoveInactiveAsync(TimeSpan offlineThreshold);
}
