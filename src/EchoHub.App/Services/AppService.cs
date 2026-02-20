using System.Text.Json;
using System.Xml.Linq;
using EchoHub.App.DTOs;
using EchoHub.App.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace EchoHub.App.Services;

public class AppService(HttpClient httpClient, IMemoryCache cache, ILogger<AppService> logger) : IAppService
{
    private const string CacheKey = "latest_version_info";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(15);

    private const string GitHubOwner = "HueByte";
    private const string GitHubRepo = "EchoHub";

    public async Task<VersionInfoDto?> GetLatestVersionAsync()
    {
        if (cache.TryGetValue(CacheKey, out VersionInfoDto? cached))
            return cached;

        try
        {
            var release = await GetLatestReleaseAsync();
            if (release is null) return null;

            var tagName = release.Value.GetProperty("tag_name").GetString()!;
            var version = await GetVersionFromBuildPropsAsync(tagName);

            var result = new VersionInfoDto
            {
                Version = version ?? tagName.TrimStart('v'),
                Url = $"https://github.com/{GitHubOwner}/{GitHubRepo}",
                Changelog = $"https://huebyte.github.io/EchoHub/changelog/{tagName}.html",
                Mandatory = false,
            };

            cache.Set(CacheKey, result, CacheDuration);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to fetch latest version info from GitHub");
            return null;
        }
    }

    private async Task<JsonElement?> GetLatestReleaseAsync()
    {
        var response = await httpClient.GetAsync(
            $"https://api.github.com/repos/{GitHubOwner}/{GitHubRepo}/releases/latest");

        if (!response.IsSuccessStatusCode) return null;

        var json = await response.Content.ReadAsStringAsync();
        return JsonDocument.Parse(json).RootElement;
    }

    private async Task<string?> GetVersionFromBuildPropsAsync(string tagName)
    {
        var response = await httpClient.GetAsync(
            $"https://raw.githubusercontent.com/{GitHubOwner}/{GitHubRepo}/{tagName}/src/Directory.Build.props");

        if (!response.IsSuccessStatusCode) return null;

        var xml = await response.Content.ReadAsStringAsync();
        var doc = XDocument.Parse(xml);
        return doc.Descendants("Version").FirstOrDefault()?.Value;
    }
}
