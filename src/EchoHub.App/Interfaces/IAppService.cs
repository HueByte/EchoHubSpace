using EchoHub.App.DTOs;

namespace EchoHub.App.Interfaces;

public interface IAppService
{
    Task<UpdateManifestDto?> GetLatestVersionAsync();
}
