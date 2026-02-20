using System.Xml.Serialization;
using EchoHub.App.DTOs;
using EchoHub.App.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EchoHub.Infrastructure.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppController(IAppService appService) : ControllerBase
{
    private static readonly XmlSerializer XmlSerializer = new(typeof(VersionInfoDto));

    [HttpGet("version")]
    public async Task<IActionResult> GetVersion([FromQuery] string? format)
    {
        var versionInfo = await appService.GetLatestVersionAsync();
        if (versionInfo is null) return StatusCode(502);

        if (string.Equals(format, "json", StringComparison.OrdinalIgnoreCase))
            return Ok(versionInfo);

        using var writer = new StringWriter();
        XmlSerializer.Serialize(writer, versionInfo);
        return Content(writer.ToString(), "application/xml");
    }
}
