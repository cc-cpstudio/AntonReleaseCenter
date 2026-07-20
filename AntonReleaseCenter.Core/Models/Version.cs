namespace AntonReleaseCenter.Core.Models;

public record Version(
    int Major,
    int Minor,
    int Build,
    int Revision
);