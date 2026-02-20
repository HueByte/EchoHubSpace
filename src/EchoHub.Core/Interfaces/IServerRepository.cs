using EchoHub.Core.Entities;

namespace EchoHub.Core.Interfaces;

/// <summary>
/// Data access layer for <see cref="Server"/> entities.
/// </summary>
public interface IServerRepository
{
    /// <summary>
    /// Retrieves all servers ordered by name.
    /// </summary>
    Task<IEnumerable<Server>> GetAllAsync();

    /// <summary>
    /// Finds a server by its unique identifier.
    /// </summary>
    /// <param name="id">The server ID.</param>
    Task<Server?> GetByIdAsync(Guid id);

    /// <summary>
    /// Finds a server by its host address.
    /// </summary>
    /// <param name="host">The host address to look up.</param>
    Task<Server?> GetByHostAsync(string host);

    /// <summary>
    /// Adds a new server to the store.
    /// </summary>
    /// <param name="server">The server entity to add.</param>
    Task<Server> AddAsync(Server server);

    /// <summary>
    /// Updates an existing server in the store.
    /// </summary>
    /// <param name="server">The server entity with updated values.</param>
    Task UpdateAsync(Server server);

    /// <summary>
    /// Deletes a server by its unique identifier.
    /// </summary>
    /// <param name="id">The server ID.</param>
    /// <returns><c>true</c> if the server was found and deleted; otherwise <c>false</c>.</returns>
    Task<bool> DeleteAsync(Guid id);

    /// <summary>
    /// Retrieves online servers whose <see cref="Server.LastSeenAt"/> is older than the given threshold.
    /// </summary>
    /// <param name="threshold">The staleness threshold relative to the current UTC time.</param>
    Task<IEnumerable<Server>> GetStaleOnlineAsync(TimeSpan threshold);

    /// <summary>
    /// Removes offline servers whose <see cref="Server.LastSeenAt"/> is older than the given threshold.
    /// </summary>
    /// <param name="offlineThreshold">The offline duration after which servers are removed.</param>
    Task RemoveInactiveAsync(TimeSpan offlineThreshold);
}
