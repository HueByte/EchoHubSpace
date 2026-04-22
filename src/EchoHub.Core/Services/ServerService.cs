using System.Security.Cryptography;
using System.Text;
using EchoHub.Core.DTOs;
using EchoHub.Core.Entities;
using EchoHub.Core.Interfaces;

namespace EchoHub.Core.Services;

/// <summary>
/// Default implementation of <see cref="IServerService"/> that manages server lifecycle operations.
/// </summary>
public class ServerService(IServerRepository serverRepository) : IServerService
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

        // Authenticated update path: caller presents a token.
        if (!string.IsNullOrWhiteSpace(dto.ClaimToken))
        {
            var tokenHash = HashToken(dto.ClaimToken);
            var existing = await serverRepository.GetByClaimTokenHashAsync(tokenHash);
            if (existing is null)
                return Fail("InvalidToken");

            var conflict = await serverRepository.FindHostConflictAsync(hosts, excludeId: existing.Id);
            if (conflict is not null)
                return Fail("HostConflict", IntersectHosts(hosts, conflict.Hosts));

            existing.Name = dto.Name;
            existing.Description = dto.Description;
            existing.Hosts = hosts;
            existing.UserCount = dto.UserCount;
            existing.Version = version;
            existing.Tags = tags;
            existing.IsOnline = true;
            existing.LastSeenAt = DateTime.UtcNow;
            await serverRepository.UpdateAsync(existing);

            return new RegisterServerOutcome(MapToDto(existing), ClaimToken: null, Error: null, ConflictingHosts: null);
        }

        // Claim path: no token presented. Any host overlap with an existing row is a hard reject —
        // legacy/grandfather adoption is intentionally absent (see claim-token protocol migration).
        var collision = await serverRepository.GetByAnyHostAsync(hosts);
        if (collision is not null)
            return Fail("HostAlreadyClaimed", IntersectHosts(hosts, collision.Hosts));

        // Fresh claim — mint token, store hash, return raw token ONCE.
        // SECURITY: never log the raw token. Only its hash or the resulting ServerId.
        var (rawToken, newHash) = GenerateClaimToken();
        var server = new Server
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            Hosts = hosts,
            UserCount = dto.UserCount,
            Version = version,
            Tags = tags,
            ClaimTokenHash = newHash,
            IsOnline = true,
            LastSeenAt = DateTime.UtcNow,
        };
        var created = await serverRepository.AddAsync(server);

        return new RegisterServerOutcome(MapToDto(created), ClaimToken: rawToken, Error: null, ConflictingHosts: null);
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

    private static (string Raw, string Hash) GenerateClaimToken()
    {
        var raw = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');
        return (raw, HashToken(raw));
    }

    private static string HashToken(string raw) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(raw))).ToLowerInvariant();

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
