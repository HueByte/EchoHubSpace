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
    /// Registers or updates a server via SignalR, marking it as online.
    /// Identity is carried by <see cref="RegisterServerDto.ClaimToken"/>; hosts are advertised endpoints.
    /// Returns the resulting <see cref="ServerDto"/> alongside a one-shot claim token on first-ever claim, or an error code.
    /// </summary>
    /// <param name="dto">The registration data sent by the server instance.</param>
    Task<RegisterServerOutcome> RegisterServerAsync(RegisterServerDto dto);

    /// <summary>
    /// Updates the connected user count for a server.
    /// </summary>
    /// <param name="id">The server ID.</param>
    /// <param name="userCount">The new user count.</param>
    /// <returns>The updated server, or <c>null</c> if the ID was not found.</returns>
    Task<ServerDto?> UpdateUserCountAsync(Guid id, int userCount);

    /// <summary>
    /// Refreshes the <c>LastSeenAt</c> timestamp for the given server to indicate it is still alive.
    /// </summary>
    /// <param name="id">The server ID.</param>
    Task RefreshLastSeenAsync(Guid id);

    /// <summary>
    /// Marks a server as offline and resets its user count to zero.
    /// </summary>
    /// <param name="id">The server ID.</param>
    Task SetServerOfflineAsync(Guid id);

    /// <summary>
    /// Deletes a server by its unique identifier.
    /// </summary>
    /// <param name="id">The server ID.</param>
    /// <returns><c>true</c> if the server was found and deleted; otherwise <c>false</c>.</returns>
    Task<bool> DeleteServerAsync(Guid id);
}
