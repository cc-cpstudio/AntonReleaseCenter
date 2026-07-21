namespace AntonReleaseCenter.Server.Controllers;

[ApiController]
[Route("api/public")]
public class PublicController : ControllerBase
{
    private readonly IReleaseService _service;

    public PublicController(IReleaseService service)
    {
        _service = service;
    }

    [HttpGet("check-update")]
    public async Task<ActionResult<CheckUpdateResponse>> CheckUpdate(
        [FromQuery] string appKey,
        [FromQuery] int channelCode,
        [FromQuery] PlatformEnum platform,
        [FromQuery] string currentVersion,
        [FromQuery] string? deviceId)
    {
        if (string.IsNullOrWhiteSpace(appKey))
            return BadRequest("appKey is required");

        if (!TryParseVersion(currentVersion, out var version))
            return BadRequest("Invalid version format. Expected: Major.Minor.Build.Revision");

        var result = await _service.CheckUpdateAsync(appKey, channelCode, platform, version, deviceId);
        if (result is null)
            return NotFound("Software or channel not found");

        return result;
    }

    private static bool TryParseVersion(string s, out Core.Models.Version version)
    {
        version = null!;
        var parts = s.Split('.');
        if (parts.Length != 4) return false;
        if (!int.TryParse(parts[0], out var major)) return false;
        if (!int.TryParse(parts[1], out var minor)) return false;
        if (!int.TryParse(parts[2], out var build)) return false;
        if (!int.TryParse(parts[3], out var revision)) return false;

        version = new Core.Models.Version(major, minor, build, revision);
        return true;
    }
}
