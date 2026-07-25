namespace AntonReleaseCenter.Server.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize]
public class AdminController : ControllerBase
{
    private readonly IAdminService _service;

    public AdminController(IAdminService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<List<AdminResponse>>> GetAll()
    {
        return await _service.GetAllAdminsAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AdminResponse>> GetById(Guid id)
    {
        var admin = await _service.GetAdminByIdAsync(id);
        if (admin is null) return NotFound();
        return admin;
    }

    [HttpPost]
    public async Task<ActionResult<AdminResponse>> Create([FromBody] CreateAdminRequest request)
    {
        var admin = await _service.CreateAdminAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = admin.AdminId }, admin);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<AdminResponse>> Update(Guid id, [FromBody] UpdateAdminRequest request)
    {
        var admin = await _service.UpdateAdminAsync(id, request);
        if (admin is null) return NotFound();
        return admin;
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        var deleted = await _service.DeleteAdminAsync(id);
        if (!deleted) return NotFound();
        return NoContent();
    }

    [HttpPost("change-password")]
    public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (adminIdClaim is null || !Guid.TryParse(adminIdClaim, out var adminId))
            return Unauthorized();

        var success = await _service.ChangePasswordAsync(adminId, request.OldPasswordHash, request.NewPasswordHash);
        if (!success) return BadRequest("旧密码错误或用户不存在");
        return NoContent();
    }
}
