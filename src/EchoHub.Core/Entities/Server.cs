namespace EchoHub.Core.Entities;

/// <summary>
/// Represents a registered EchoHub server instance.
/// </summary>
public class Server
{
    /// <summary>
    /// Gets or sets the unique identifier of the server.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the display name of the server.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets an optional description of the server.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the host address used to connect to the server.
    /// </summary>
    public required string Host { get; set; }

    /// <summary>
    /// Gets or sets the number of users currently connected to the server.
    /// </summary>
    public int UserCount { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the server is currently online.
    /// </summary>
    public bool IsOnline { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp of the last heartbeat or activity from the server.
    /// </summary>
    public DateTime LastSeenAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the UTC timestamp when the server was first registered.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
