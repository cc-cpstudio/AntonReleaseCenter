using AntonReleaseCenter.Server.Models;

namespace AntonReleaseCenter.Server.Controllers;

[ApiController]
[Route("api")]
public class ReleaseController : ControllerBase
{
    private readonly IReleaseService _service;
    private static readonly string UploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

    public ReleaseController(IReleaseService service)
    {
        _service = service;
    }

    [HttpGet("software/{softwareId}/releases")]
    public async Task<ActionResult<List<SoftwareRelease>>> GetBySoftware(Guid softwareId, [FromQuery] Guid? channelId)
    {
        return await _service.GetReleasesBySoftwareIdAsync(softwareId, channelId);
    }

    [HttpGet("software/{softwareName}/releases/{platform}")]
    public async Task<ActionResult<List<SoftwareRelease>>> GetBySoftwareNameAndPlatform(
        string softwareName, PlatformEnum platform)
    {
        return await _service.GetReleasesBySoftwareNameAndPlatformAsync(softwareName, platform);
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
    [DisableRequestSizeLimit]
    public async Task<ActionResult<SoftwareRelease>> Create(Guid softwareId, [FromForm] CreateReleaseFormRequest form)
    {
        var (filePath, fileSize, fileHash) = await SaveFileAsync(form.File);

        var request = new CreateReleaseRequest(
            softwareId,
            form.ChannelId,
            form.Platform,
            Version.Parse(form.Version),
            form.UpdateLog,
            filePath,
            fileSize,
            fileHash,
            form.IsForceUpdate,
            form.IsOnline
        );

        var release = await _service.CreateReleaseAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = release.SoftwareReleaseId }, release);
    }

    [HttpPut("releases/{id}")]
    [Authorize]
    [DisableRequestSizeLimit]
    public async Task<ActionResult<SoftwareRelease>> Update(Guid id, [FromForm] UpdateReleaseFormRequest form)
    {
        var existing = await _service.GetReleaseByIdAsync(id);
        if (existing is null) return NotFound();

        var (filePath, fileSize, fileHash) = await SaveFileAsync(form.File, existing.FilePath, existing.FileSize, existing.FileHash);

        var request = new UpdateReleaseRequest(
            form.ChannelId,
            form.Platform,
            Version.Parse(form.Version),
            form.UpdateLog,
            filePath,
            fileSize,
            fileHash,
            form.IsForceUpdate,
            form.IsOnline
        );

        var updated = await _service.UpdateReleaseAsync(id, request);
        return updated!;
    }

    [HttpDelete("releases/{id}")]
    [Authorize]
    public async Task<ActionResult> Delete(Guid id)
    {
        var release = await _service.GetReleaseByIdAsync(id);
        if (release is null) return NotFound();

        DeleteFile(release.FilePath);

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

    private async Task<(string filePath, int fileSize, string fileHash)> SaveFileAsync(
        IFormFile? file, string? oldPath = null, int oldSize = 0, string? oldHash = null)
    {
        if (file is null || file.Length == 0)
            return oldPath is not null ? (oldPath, oldSize, oldHash ?? "") : ("", 0, "");

        if (oldPath is not null)
            DeleteFile(oldPath);

        Directory.CreateDirectory(UploadsDir);

        var ext = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid()}{ext}";
        var fullPath = Path.Combine(UploadsDir, fileName);

        await using var writeStream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(writeStream);
        await writeStream.FlushAsync();

        var size = (int)writeStream.Length;

        await using var readStream = System.IO.File.OpenRead(fullPath);
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hashBytes = await sha256.ComputeHashAsync(readStream);
        var hash = Convert.ToHexStringLower(hashBytes);

        return ($"/uploads/{fileName}", size, hash);
    }

    private static void DeleteFile(string? filePath)
    {
        if (string.IsNullOrEmpty(filePath)) return;
        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", filePath.TrimStart('/'));
        if (System.IO.File.Exists(fullPath))
            System.IO.File.Delete(fullPath);
    }
}
