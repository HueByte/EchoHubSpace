using EchoHub.Core.DTOs;
using EchoHub.Core.Interfaces;
using EchoHub.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace EchoHub.App.Controllers;

/// <summary>
/// REST endpoints for querying and managing registered EchoHub servers.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ServersController(IServerService serverService) : ControllerBase
{
    /// <summary>
    /// Returns all registered servers.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ServerDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ServerDto>>>> GetAll()
    {
        var servers = await serverService.GetAllServersAsync();
        return Ok(ApiResponse.Ok(servers));
    }

    /// <summary>
    /// Returns a single server by its unique identifier.
    /// </summary>
    /// <param name="id">The server ID.</param>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ServerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ServerDto>>> GetById(Guid id)
    {
        var server = await serverService.GetServerByIdAsync(id);
        if (server is null)
            return NotFound(ApiResponse.Fail("Server not found"));

        return Ok(ApiResponse.Ok(server));
    }

    /// <summary>
    /// Creates a new server entry.
    /// </summary>
    /// <param name="dto">The server creation data.</param>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ServerDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<ServerDto>>> Create(CreateServerDto dto)
    {
        var server = await serverService.CreateServerAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = server.Id }, ApiResponse.Ok(server));
    }

    /// <summary>
    /// Deletes a server by its unique identifier.
    /// </summary>
    /// <param name="id">The server ID.</param>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id)
    {
        var deleted = await serverService.DeleteServerAsync(id);
        if (!deleted)
            return NotFound(ApiResponse.Fail("Server not found"));

        return Ok(ApiResponse.Ok("Server deleted"));
    }
}
