using System.Xml.Serialization;
using EchoHub.Core.DTOs;
using EchoHub.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EchoHub.App.Controllers;

/// <summary>
/// Provides application-level endpoints such as version checking.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AppController(IAppService appService) : ControllerBase
{
    private static readonly XmlSerializer XmlSerializer = new(typeof(UpdateManifestDto));

    /// <summary>
    /// Returns the latest version information from GitHub releases.
    /// Defaults to XML format; pass <c>format=json</c> for JSON.
    /// </summary>
    /// <param name="format">The desired response format ("json" or "xml"). Defaults to XML.</param>
    [HttpGet("version")]
    public async Task<IActionResult> GetVersion([FromQuery] string? format)
    {
        var manifest = await appService.GetLatestVersionAsync();
        if (manifest is null)
            return StatusCode(500, "Failed to fetch version info");

        if (string.Equals(format, "json", StringComparison.OrdinalIgnoreCase))
            return Ok(manifest);

        using var writer = new StringWriter();
        XmlSerializer.Serialize(writer, manifest);
        return Content(writer.ToString(), "application/xml");
    }
}
