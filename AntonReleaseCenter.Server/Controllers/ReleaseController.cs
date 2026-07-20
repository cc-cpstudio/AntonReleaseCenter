namespace AntonReleaseCenter.Server.Controllers;

[ApiController]
[Route("api")]
public class ReleaseController : ControllerBase
{
    private readonly IReleaseService _service;

    public ReleaseController(IReleaseService service)
    {
        _service = service;
    }

    [HttpGet("software/{softwareId}/releases")]
    public async Task<ActionResult<List<SoftwareRelease>>> GetBySoftware(Guid softwareId, [FromQuery] Guid? channelId)
    {
        return await _service.GetReleasesBySoftwareIdAsync(softwareId, channelId);
    }

    [HttpGet("releases/{id}")]
    public async Task<ActionResult<SoftwareRelease>> GetById(Guid id)
    {
        var release = await _service.GetReleaseByIdAsync(id);
        if (release is null) return NotFound();
        return release;
    }

    [HttpPost("software/{softwareId}/releases")]
    [Authorize]
    public async Task<ActionResult<SoftwareRelease>> Create(Guid softwareId, [FromBody] CreateReleaseRequest request)
    {
        if (request.SoftwareId != softwareId)
            return BadRequest("SoftwareId mismatch");

        var release = await _service.CreateReleaseAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = release.SoftwareReleaseId }, release);
    }

    [HttpPut("releases/{id}")]
    [Authorize]
    public async Task<ActionResult<SoftwareRelease>> Update(Guid id, [FromBody] UpdateReleaseRequest request)
    {
        var release = await _service.UpdateReleaseAsync(id, request);
        if (release is null) return NotFound();
        return release;
    }

    [HttpDelete("releases/{id}")]
    [Authorize]
    public async Task<ActionResult> Delete(Guid id)
    {
        var deleted = await _service.DeleteReleaseAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }

    [HttpPatch("releases/{id}/toggle-online")]
    [Authorize]
    public async Task<ActionResult<SoftwareRelease>> ToggleOnline(Guid id)
    {
        var release = await _service.ToggleReleaseOnlineAsync(id);
        if (release is null) return NotFound();
        return release;
    }
}
