using AntonReleaseCenter.Core.Models;

namespace AntonReleaseCenter.SoftwareSDK.Model;

public record Configure
{
    public required string Url { get; init; }
    public required Guid SoftwareId { get; init; }
}