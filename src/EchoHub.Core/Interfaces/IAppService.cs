using EchoHub.Core.DTOs;

namespace EchoHub.Core.Interfaces;

public interface IAppService
{
    Task<UpdateManifestDto?> GetLatestVersionAsync();
}
