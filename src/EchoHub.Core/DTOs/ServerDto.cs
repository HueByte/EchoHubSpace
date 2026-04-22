namespace EchoHub.Core.DTOs;

/// <summary>
/// Represents server information returned to clients.
/// </summary>
/// <param name="Id">The unique identifier of the server.</param>
/// <param name="Name">The display name of the server.</param>
/// <param name="Description">An optional description of the server.</param>
/// <param name="Hosts">The host addresses the server is reachable at.</param>
/// <param name="UserCount">The number of users currently connected.</param>
/// <param name="Version">The version string reported by the server.</param>
/// <param name="Tags">The tags advertised by the server.</param>
/// <param name="IsOnline">Whether the server is currently online.</param>
/// <param name="CreatedAt">The UTC timestamp when the server was first registered.</param>
public record ServerDto(
    Guid Id,
    string Name,
    string? Description,
    string[] Hosts,
    int UserCount,
    string Version,
    string[] Tags,
    bool IsOnline,
    DateTime CreatedAt
);

/// <summary>
/// Data sent by an EchoHub server instance to register or update itself via SignalR.
/// </summary>
/// <param name="Name">The display name of the server.</param>
/// <param name="Description">An optional description of the server.</param>
/// <param name="Hosts">The host addresses the server is reachable at.</param>
/// <param name="UserCount">The current number of connected users.</param>
/// <param name="Version">The version string reported by the server.</param>
/// <param name="Tags">The tags advertised by the server.</param>
/// <param name="ClaimToken">The previously-issued claim token, or null on first-ever registration.</param>
public record RegisterServerDto(
    string Name,
    string? Description,
    string[] Hosts,
    int UserCount,
    string Version,
    string[] Tags,
    string? ClaimToken);

/// <summary>
/// Result returned to the client after a <c>RegisterServer</c> hub call.
/// </summary>
/// <param name="Success">Whether the registration succeeded.</param>
/// <param name="ServerId">The server's directory identifier, when successful. Stable across reconnects.</param>
/// <param name="ClaimToken">The newly-issued claim token — populated only when a brand-new row was created in this call. Null on every subsequent update.</param>
/// <param name="Error">Failure reason code when <paramref name="Success"/> is false. One of <c>HostAlreadyClaimed</c>, <c>InvalidToken</c>, <c>HostConflict</c>, <c>InvalidInput</c>.</param>
/// <param name="ConflictingHosts">On <c>HostAlreadyClaimed</c> and <c>HostConflict</c>, the hosts from the request that collided with another row. Null otherwise.</param>
public record RegisterServerResult(
    bool Success,
    Guid? ServerId,
    string? ClaimToken,
    string? Error,
    string[]? ConflictingHosts);

/// <summary>
/// Internal service-layer outcome for a register call. The hub maps this to <see cref="RegisterServerResult"/> for the wire.
/// </summary>
/// <param name="Server">The resulting server, when successful. Null on failure.</param>
/// <param name="ClaimToken">Newly-issued raw claim token — populated only on fresh-row creation. Null on updates.</param>
/// <param name="Error">Failure code when <paramref name="Server"/> is null.</param>
/// <param name="ConflictingHosts">Hosts from the request that collided with another row, on host-conflict errors.</param>
public record RegisterServerOutcome(
    ServerDto? Server,
    string? ClaimToken,
    string? Error,
    string[]? ConflictingHosts);
