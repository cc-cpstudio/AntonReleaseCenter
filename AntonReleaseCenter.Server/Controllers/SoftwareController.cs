namespace AntonReleaseCenter.Server.Controllers;

[ApiController]
[Route("api/software")]
public class SoftwareController : ControllerBase
{
    private readonly IReleaseService _service;

    public SoftwareController(IReleaseService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<Software>>> GetAll()
    {
        return await _service.GetAllSoftwareAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Software>> GetById(Guid id)
    {
        var software = await _service.GetSoftwareByIdAsync(id);
        if (software is null) return NotFound();
        return software;
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<Software>> Create([FromBody] CreateSoftwareRequest request)
    {
        var software = await _service.CreateSoftwareAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = software.SoftwareId }, software);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<Software>> Update(Guid id, [FromBody] UpdateSoftwareRequest request)
    {
        var software = await _service.UpdateSoftwareAsync(id, request);
        if (software is null) return NotFound();
        return software;
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<ActionResult> Delete(Guid id)
    {
        var deleted = await _service.DeleteSoftwareAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }
}
