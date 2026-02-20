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

    private static readonly Dictionary<string, string> AssetOsMap = new()
    {
        ["EchoHub-Client-win-x64.zip"] = "windows",
        ["EchoHub-Client-linux-x64.zip"] = "linux",
        ["EchoHub-Client-osx-x64.zip"] = "osx-x64",
        ["EchoHub-Client-osx-arm64.zip"] = "osx-arm64",
    };

    public async Task<UpdateManifestDto?> GetLatestVersionAsync()
    {
        if (cache.TryGetValue(CacheKey, out UpdateManifestDto? cached))
            return cached;

        try
        {
            var release = await GetLatestReleaseAsync();
            if (release is null) return null;

            var tagName = release.Value.GetProperty("tag_name").GetString()!;
            var version = await GetVersionFromBuildPropsAsync(tagName)
                          ?? tagName.TrimStart('v');
            var changelog = $"https://huebyte.github.io/EchoHub/changelog/{tagName}.html";

            var items = new List<UpdateItemDto>();

            foreach (var asset in release.Value.GetProperty("assets").EnumerateArray())
            {
                var name = asset.GetProperty("name").GetString()!;
                if (!AssetOsMap.TryGetValue(name, out var os)) continue;

                var digest = asset.GetProperty("digest").GetString() ?? string.Empty;
                ParseDigest(digest, out var algorithm, out var hash);

                items.Add(new UpdateItemDto
                {
                    Os = os,
                    Version = version,
                    Url = asset.GetProperty("browser_download_url").GetString()!,
                    Changelog = changelog,
                    Mandatory = false,
                    Checksum = new ChecksumDto { Algorithm = algorithm, Value = hash },
                });
            }

            var result = new UpdateManifestDto { Items = items };

            cache.Set(CacheKey, result, CacheDuration);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to fetch latest version info from GitHub");
            return null;
        }
    }

    private static void ParseDigest(string digest, out string algorithm, out string hash)
    {
        var parts = digest.Split(':', 2);
        if (parts.Length == 2)
        {
            algorithm = parts[0].ToUpperInvariant();
            hash = parts[1];
        }
        else
        {
            algorithm = string.Empty;
            hash = digest;
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
