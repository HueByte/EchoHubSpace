using EchoHub.Core.DTOs;
using EchoHub.Core.Entities;
using EchoHub.Core.Interfaces;

namespace EchoHub.Core.Services;

/// <summary>
/// Default implementation of <see cref="IServerService"/> that manages server lifecycle operations.
/// </summary>
public class ServerService(
    IServerRepository serverRepository,
    IClaimTokenService tokenService) : IServerService
{
    private const int MaxTagsPerServer = 10;

    /// <inheritdoc />
    public async Task<IEnumerable<ServerDto>> GetAllServersAsync()
    {
        var servers = await serverRepository.GetAllAsync();
        return servers.Select(MapToDto);
    }

    /// <inheritdoc />
    public async Task<ServerDto?> GetServerByIdAsync(Guid id)
    {
        var server = await serverRepository.GetByIdAsync(id);
        return server is null ? null : MapToDto(server);
    }

    /// <inheritdoc />
    public async Task<RegisterServerOutcome> RegisterServerAsync(RegisterServerDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return Fail("InvalidInput");

        var hosts = NormalizeSet(dto.Hosts);
        if (hosts.Count == 0)
            return Fail("InvalidInput");

        var tags = NormalizeSet(dto.Tags).Take(MaxTagsPerServer).ToList();
        var version = string.IsNullOrWhiteSpace(dto.Version) ? "unknown" : dto.Version.Trim();

        // Authenticated path: caller presents a signed token.
        if (!string.IsNullOrWhiteSpace(dto.ClaimToken))
        {
            if (!tokenService.TryVerify(dto.ClaimToken, out var serverId))
                return Fail("InvalidToken");

            var existing = await serverRepository.GetByIdAsync(serverId);
            if (existing is not null)
            {
                var conflict = await serverRepository.FindHostConflictAsync(hosts, excludeId: existing.Id);
                if (conflict is not null)
                    return Fail("HostConflict", IntersectHosts(hosts, conflict.Hosts));

                Apply(existing, dto.Name, dto.Description, hosts, dto.UserCount, version, tags);
                await serverRepository.UpdateAsync(existing);
                return Success(existing);
            }

            // Row doesn't exist — either the API restarted (InMemory state lost) or an admin
            // deleted the row. The signature is valid, so the caller is who they claim to be:
            // re-create the row with the embedded serverId so ServerId stays stable.
            var freshConflict = await serverRepository.GetByAnyHostAsync(hosts);
            if (freshConflict is not null)
                return Fail("HostConflict", IntersectHosts(hosts, freshConflict.Hosts));

            var restored = new Server
            {
                Id = serverId,
                Name = dto.Name,
                Description = dto.Description,
                Hosts = hosts,
                UserCount = dto.UserCount,
                Version = version,
                Tags = tags,
                IsOnline = true,
                LastSeenAt = DateTime.UtcNow,
            };
            var createdRestored = await serverRepository.AddAsync(restored);
            // No new token — the caller's existing token is still valid.
            return Success(createdRestored);
        }

        // Claim path: no token presented. Any host overlap with an existing row is a hard reject —
        // new tokens are only minted here.
        var collision = await serverRepository.GetByAnyHostAsync(hosts);
        if (collision is not null)
            return Fail("HostAlreadyClaimed", IntersectHosts(hosts, collision.Hosts));

        var newId = Guid.NewGuid();
        // SECURITY: the raw token is returned to the caller once, wrapped in the response envelope.
        // Never log it; only the ServerId appears in logs.
        var token = tokenService.Issue(newId);
        var server = new Server
        {
            Id = newId,
            Name = dto.Name,
            Description = dto.Description,
            Hosts = hosts,
            UserCount = dto.UserCount,
            Version = version,
            Tags = tags,
            IsOnline = true,
            LastSeenAt = DateTime.UtcNow,
        };
        var created = await serverRepository.AddAsync(server);
        return new RegisterServerOutcome(MapToDto(created), ClaimToken: token, Error: null, ConflictingHosts: null);
    }

    /// <inheritdoc />
    public async Task<ServerDto?> UpdateUserCountAsync(Guid id, int userCount)
    {
        var server = await serverRepository.GetByIdAsync(id);
        if (server is null) return null;

        server.UserCount = userCount;
        server.LastSeenAt = DateTime.UtcNow;
        await serverRepository.UpdateAsync(server);
        return MapToDto(server);
    }

    /// <inheritdoc />
    public async Task RefreshLastSeenAsync(Guid id)
    {
        var server = await serverRepository.GetByIdAsync(id);
        if (server is null) return;

        server.LastSeenAt = DateTime.UtcNow;
        await serverRepository.UpdateAsync(server);
    }

    /// <inheritdoc />
    public async Task SetServerOfflineAsync(Guid id)
    {
        var server = await serverRepository.GetByIdAsync(id);
        if (server is not null)
        {
            server.IsOnline = false;
            server.UserCount = 0;
            await serverRepository.UpdateAsync(server);
        }
    }

    /// <inheritdoc />
    public async Task<bool> DeleteServerAsync(Guid id)
    {
        return await serverRepository.DeleteAsync(id);
    }

    private static void Apply(Server server, string name, string? description, List<string> hosts,
        int userCount, string version, List<string> tags)
    {
        server.Name = name;
        server.Description = description;
        server.Hosts = hosts;
        server.UserCount = userCount;
        server.Version = version;
        server.Tags = tags;
        server.IsOnline = true;
        server.LastSeenAt = DateTime.UtcNow;
    }

    private static RegisterServerOutcome Success(Server server) =>
        new(MapToDto(server), ClaimToken: null, Error: null, ConflictingHosts: null);

    private static RegisterServerOutcome Fail(string error, string[]? conflictingHosts = null) =>
        new(Server: null, ClaimToken: null, Error: error, ConflictingHosts: conflictingHosts);

    private static List<string> NormalizeSet(IEnumerable<string>? values) =>
        (values ?? [])
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Select(v => v.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

    private static string[] IntersectHosts(IEnumerable<string> requested, IEnumerable<string> existing)
    {
        var existingSet = existing.ToHashSet(StringComparer.OrdinalIgnoreCase);
        return requested.Where(existingSet.Contains).ToArray();
    }

    private static ServerDto MapToDto(Server server) =>
        new(
            server.Id,
            server.Name,
            server.Description,
            server.Hosts.ToArray(),
            server.UserCount,
            server.Version,
            server.Tags.ToArray(),
            server.IsOnline,
            server.CreatedAt
        );
}
