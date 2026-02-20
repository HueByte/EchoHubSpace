using EchoHub.App.DTOs;

namespace EchoHub.App.Interfaces;

public interface IAppService
{
    Task<VersionInfoDto?> GetLatestVersionAsync();
}
