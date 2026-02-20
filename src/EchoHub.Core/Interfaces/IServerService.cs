using EchoHub.Core.DTOs;

namespace EchoHub.Core.Interfaces;

/// <summary>
/// Domain service for managing EchoHub server registrations and state.
/// </summary>
public interface IServerService
{
    /// <summary>
    /// Retrieves all registered servers.
    /// </summary>
    Task<IEnumerable<ServerDto>> GetAllServersAsync();

    /// <summary>
    /// Retrieves a server by its unique identifier.
    /// </summary>
    /// <param name="id">The server ID.</param>
    Task<ServerDto?> GetServerByIdAsync(Guid id);

    /// <summary>
    /// Creates a new server entry (initially offline).
    /// </summary>
    /// <param name="dto">The server creation data.</param>
    Task<ServerDto> CreateServerAsync(CreateServerDto dto);

    /// <summary>
    /// Registers or updates a server via SignalR, marking it as online.
    /// </summary>
    /// <param name="dto">The registration data sent by the server instance.</param>
    Task<ServerDto> RegisterServerAsync(RegisterServerDto dto);

    /// <summary>
    /// Updates the connected user count for a server identified by host.
    /// </summary>
    /// <param name="host">The host address of the server.</param>
    /// <param name="userCount">The new user count.</param>
    /// <returns>The updated server, or <c>null</c> if the host was not found.</returns>
    Task<ServerDto?> UpdateUserCountAsync(string host, int userCount);

    /// <summary>
    /// Refreshes the <c>LastSeenAt</c> timestamp for the given host to indicate it is still alive.
    /// </summary>
    /// <param name="host">The host address of the server.</param>
    Task RefreshLastSeenAsync(string host);

    /// <summary>
    /// Marks a server as offline and resets its user count to zero.
    /// </summary>
    /// <param name="host">The host address of the server.</param>
    Task SetServerOfflineAsync(string host);

    /// <summary>
    /// Deletes a server by its unique identifier.
    /// </summary>
    /// <param name="id">The server ID.</param>
    /// <returns><c>true</c> if the server was found and deleted; otherwise <c>false</c>.</returns>
    Task<bool> DeleteServerAsync(Guid id);
}
