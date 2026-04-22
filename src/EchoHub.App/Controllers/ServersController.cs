using EchoHub.App.Filters;
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
    [ProducesResponseType(typeof(Response<IEnumerable<ServerDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<Response<IEnumerable<ServerDto>>>> GetAll()
    {
        var servers = await serverService.GetAllServersAsync();
        return Ok(Respond.Ok(servers));
    }

    /// <summary>
    /// Returns a single server by its unique identifier.
    /// </summary>
    /// <param name="id">The server ID.</param>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(Response<ServerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Response<ServerDto>>> GetById(Guid id)
    {
        var server = await serverService.GetServerByIdAsync(id);
        if (server is null)
            return NotFound(Respond.Fail("NotFound", "Server not found"));

        return Ok(Respond.Ok(server));
    }

    /// <summary>
    /// Deletes a server by its unique identifier. Requires API key authorization.
    /// </summary>
    /// <param name="id">The server ID.</param>
    [HttpDelete("{id:guid}")]
    [ApiKeyAuth]
    [ProducesResponseType(typeof(Response), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Response), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Response>> Delete(Guid id)
    {
        var deleted = await serverService.DeleteServerAsync(id);
        if (!deleted)
            return NotFound(Respond.Fail("NotFound", "Server not found"));

        return Ok(Respond.Ok());
    }
}
