namespace AntonReleaseCenter.Core.DTOs;

public record CreateReleaseRequest(
    Guid SoftwareId,
    Guid ChannelId,
    Version Version,
    string UpdateLog,
    string FilePath,
    int FileSize,
    string FileHash,
    bool IsForceUpdate,
    bool IsOnline
);

public record UpdateReleaseRequest(
    Guid ChannelId,
    Version Version,
    string UpdateLog,
    string FilePath,
    int FileSize,
    string FileHash,
    bool IsForceUpdate,
    bool IsOnline
);
