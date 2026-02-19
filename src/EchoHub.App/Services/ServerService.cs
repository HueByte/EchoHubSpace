using EchoHub.App.DTOs;
using EchoHub.App.Interfaces;
using EchoHub.Core.Entities;
using EchoHub.Core.Interfaces;

namespace EchoHub.App.Services;

public class ServerService(IServerRepository serverRepository) : IServerService
{
    public async Task<IEnumerable<ServerDto>> GetAllServersAsync()
    {
        var servers = await serverRepository.GetAllAsync();
        return servers.Select(MapToDto);
    }

    public async Task<ServerDto?> GetServerByIdAsync(Guid id)
    {
        var server = await serverRepository.GetByIdAsync(id);
        return server is null ? null : MapToDto(server);
    }

    public async Task<ServerDto> CreateServerAsync(CreateServerDto dto)
    {
        var server = new Server
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            Host = dto.Host,
            IsOnline = false,
            UserCount = 0,
        };

        var created = await serverRepository.AddAsync(server);
        return MapToDto(created);
    }

    public async Task<ServerDto> RegisterServerAsync(RegisterServerDto dto)
    {
        var existing = await serverRepository.GetByHostAsync(dto.Host);

        if (existing is not null)
        {
            existing.Name = dto.Name;
            existing.Description = dto.Description;
            existing.UserCount = dto.UserCount;
            existing.IsOnline = true;
            await serverRepository.UpdateAsync(existing);
            return MapToDto(existing);
        }

        var server = new Server
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description,
            Host = dto.Host,
            UserCount = dto.UserCount,
            IsOnline = true,
        };

        var created = await serverRepository.AddAsync(server);
        return MapToDto(created);
    }

    public async Task SetServerOfflineAsync(string host)
    {
        var server = await serverRepository.GetByHostAsync(host);
        if (server is not null)
        {
            server.IsOnline = false;
            server.UserCount = 0;
            await serverRepository.UpdateAsync(server);
        }
    }

    public async Task DeleteServerAsync(Guid id)
    {
        await serverRepository.DeleteAsync(id);
    }

    private static ServerDto MapToDto(Server server) =>
        new(
            server.Id,
            server.Name,
            server.Description,
            server.Host,
            server.UserCount,
            server.IsOnline,
            server.CreatedAt
        );
}
