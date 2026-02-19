using EchoHub.App.DTOs;

namespace EchoHub.App.Interfaces;

public interface IServerService
{
    Task<IEnumerable<ServerDto>> GetAllServersAsync();
    Task<ServerDto?> GetServerByIdAsync(Guid id);
    Task<ServerDto> CreateServerAsync(CreateServerDto dto);
    Task<ServerDto> RegisterServerAsync(RegisterServerDto dto);
    Task<ServerDto?> UpdateUserCountAsync(string host, int userCount);
    Task SetServerOfflineAsync(string host);
    Task<bool> DeleteServerAsync(Guid id);
}
