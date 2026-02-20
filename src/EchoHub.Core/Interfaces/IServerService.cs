using EchoHub.Core.DTOs;

namespace EchoHub.Core.Interfaces;

public interface IServerService
{
    Task<IEnumerable<ServerDto>> GetAllServersAsync();
    Task<ServerDto?> GetServerByIdAsync(Guid id);
    Task<ServerDto> CreateServerAsync(CreateServerDto dto);
    Task<ServerDto> RegisterServerAsync(RegisterServerDto dto);
    Task<ServerDto?> UpdateUserCountAsync(string host, int userCount);
    Task RefreshLastSeenAsync(string host);
    Task SetServerOfflineAsync(string host);
    Task<bool> DeleteServerAsync(Guid id);
}
