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
public record RegisterServerDto(
    string Name,
    string? Description,
    string[] Hosts,
    int UserCount,
    string Version,
    string[] Tags);
