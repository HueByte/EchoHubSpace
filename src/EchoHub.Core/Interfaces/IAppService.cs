using EchoHub.Core.DTOs;

namespace EchoHub.Core.Interfaces;

/// <summary>
/// Provides application-level services such as version information retrieval.
/// </summary>
public interface IAppService
{
    /// <summary>
    /// Retrieves the latest version information from GitHub releases and returns it as an update manifest.
    /// </summary>
    /// <returns>The update manifest, or <c>null</c> if the version info could not be fetched.</returns>
    Task<UpdateManifestDto?> GetLatestVersionAsync();
}
