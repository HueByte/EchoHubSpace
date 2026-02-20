using EchoHub.Core.DTOs;
using EchoHub.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EchoHub.App.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServersController(IServerService serverService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ServerDto>>> GetAll()
    {
        var servers = await serverService.GetAllServersAsync();
        return Ok(servers);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ServerDto>> GetById(Guid id)
    {
        var server = await serverService.GetServerByIdAsync(id);
        return server is null ? NotFound() : Ok(server);
    }

    [HttpPost]
    public async Task<ActionResult<ServerDto>> Create(CreateServerDto dto)
    {
        var server = await serverService.CreateServerAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = server.Id }, server);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await serverService.DeleteServerAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
