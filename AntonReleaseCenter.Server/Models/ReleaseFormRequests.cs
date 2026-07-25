using AntonReleaseCenter.Core.Models;

namespace AntonReleaseCenter.Server.Models;

public class CreateReleaseFormRequest
{
    public Guid ChannelId { get; set; }
    public PlatformEnum Platform { get; set; }
    public string Version { get; set; } = string.Empty;
    public string UpdateLog { get; set; } = string.Empty;
    public bool IsForceUpdate { get; set; }
    public bool IsOnline { get; set; } = true;
    public IFormFile? File { get; set; }
}

public class UpdateReleaseFormRequest
{
    public Guid ChannelId { get; set; }
    public PlatformEnum Platform { get; set; }
    public string Version { get; set; } = string.Empty;
    public string UpdateLog { get; set; } = string.Empty;
    public bool IsForceUpdate { get; set; }
    public bool IsOnline { get; set; } = true;
    public IFormFile? File { get; set; }
}
