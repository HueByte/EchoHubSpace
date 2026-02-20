using System.Xml.Serialization;
using EchoHub.App.DTOs;
using EchoHub.App.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EchoHub.Infrastructure.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppController(IAppService appService) : ControllerBase
{
    private static readonly XmlSerializer XmlSerializer = new(typeof(UpdateManifestDto));

    [HttpGet("version")]
    public async Task<IActionResult> GetVersion([FromQuery] string? format)
    {
        var manifest = await appService.GetLatestVersionAsync();
        if (manifest is null) return StatusCode(502);

        if (string.Equals(format, "json", StringComparison.OrdinalIgnoreCase))
            return Ok(manifest);

        using var writer = new StringWriter();
        XmlSerializer.Serialize(writer, manifest);
        return Content(writer.ToString(), "application/xml");
    }
}
