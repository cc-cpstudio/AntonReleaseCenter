namespace AntonReleaseCenter.Server.Controllers;

[ApiController]
[Route("api/software/{softwareId}/channels")]
public class ChannelController : ControllerBase
{
    private readonly IReleaseService _service;

    public ChannelController(IReleaseService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<Channel>>> GetBySoftware(Guid softwareId)
    {
        return await _service.GetChannelsBySoftwareIdAsync(softwareId);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Channel>> GetById(Guid id)
    {
        var channel = await _service.GetChannelByIdAsync(id);
        if (channel is null) return NotFound();
        return channel;
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<Channel>> Create(Guid softwareId, [FromBody] CreateChannelRequest request)
    {
        var channel = await _service.CreateChannelAsync(softwareId, request);
        return CreatedAtAction(nameof(GetById), new { softwareId, id = channel.ChannelId }, channel);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<Channel>> Update(Guid id, [FromBody] UpdateChannelRequest request)
    {
        var channel = await _service.UpdateChannelAsync(id, request);
        if (channel is null) return NotFound();
        return channel;
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<ActionResult> Delete(Guid id)
    {
        var deleted = await _service.DeleteChannelAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
