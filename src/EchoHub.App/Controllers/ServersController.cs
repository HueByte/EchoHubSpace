using EchoHub.Core.DTOs;
using EchoHub.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EchoHub.App.Controllers;

/// <summary>
/// REST endpoints for querying and managing registered EchoHub servers.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ServersController(IServerService serverService) : ControllerBase
{
    /// <summary>
    /// Returns all registered servers.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ServerDto>>> GetAll()
    {
        var servers = await serverService.GetAllServersAsync();
        return Ok(servers);
    }

    /// <summary>
    /// Returns a single server by its unique identifier.
    /// </summary>
    /// <param name="id">The server ID.</param>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ServerDto>> GetById(Guid id)
    {
        var server = await serverService.GetServerByIdAsync(id);
        return server is null ? NotFound() : Ok(server);
    }

    /// <summary>
    /// Creates a new server entry.
    /// </summary>
    /// <param name="dto">The server creation data.</param>
    [HttpPost]
    public async Task<ActionResult<ServerDto>> Create(CreateServerDto dto)
    {
        var server = await serverService.CreateServerAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = server.Id }, server);
    }

    /// <summary>
    /// Deletes a server by its unique identifier.
    /// </summary>
    /// <param name="id">The server ID.</param>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await serverService.DeleteServerAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
