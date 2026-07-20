namespace AntonReleaseCenter.Server.Services;

public class ReleaseService : IReleaseService
{
    private readonly AppDbContext _db;

    public ReleaseService(AppDbContext db)
    {
        _db = db;
    }

    // ===== Software =====

    public async Task<List<Software>> GetAllSoftwareAsync()
    {
        return await _db.Software.ToListAsync();
    }

    public async Task<Software?> GetSoftwareByIdAsync(Guid id)
    {
        return await _db.Software.FindAsync(id);
    }

    public async Task<Software> CreateSoftwareAsync(CreateSoftwareRequest request)
    {
        var software = new Software(
            Guid.NewGuid(),
            request.AppKey,
            request.Name,
            request.Description,
            request.IsEnabled
        );
        _db.Software.Add(software);
        await _db.SaveChangesAsync();
        return software;
    }

    public async Task<Software?> UpdateSoftwareAsync(Guid id, UpdateSoftwareRequest request)
    {
        var software = await _db.Software.FindAsync(id);
        if (software is null) return null;

        _db.Software.Entry(software).CurrentValues.SetValues(new Software(
            id,
            software.AppKey,
            request.Name,
            request.Description,
            request.IsEnabled
        ));
        await _db.SaveChangesAsync();
        return software;
    }

    public async Task<bool> DeleteSoftwareAsync(Guid id)
    {
        var software = await _db.Software.FindAsync(id);
        if (software is null) return false;

        _db.Software.Remove(software);
        await _db.SaveChangesAsync();
        return true;
    }

    // ===== Channel =====

    public async Task<List<Channel>> GetChannelsBySoftwareIdAsync(Guid softwareId)
    {
        return await _db.Channels.Where(c => c.SoftwareId == softwareId).ToListAsync();
    }

    public async Task<Channel?> GetChannelByIdAsync(Guid id)
    {
        return await _db.Channels.FindAsync(id);
    }

    public async Task<Channel> CreateChannelAsync(Guid softwareId, CreateChannelRequest request)
    {
        var channel = new Channel(
            Guid.NewGuid(),
            softwareId,
            request.ChannelCode,
            request.ChannelName,
            request.GrayScalePercent
        );
        _db.Channels.Add(channel);
        await _db.SaveChangesAsync();
        return channel;
    }

    public async Task<Channel?> UpdateChannelAsync(Guid id, UpdateChannelRequest request)
    {
        var channel = await _db.Channels.FindAsync(id);
        if (channel is null) return null;

        _db.Channels.Entry(channel).CurrentValues.SetValues(new Channel(
            id,
            channel.SoftwareId,
            channel.ChannelCode,
            request.ChannelName,
            request.GrayScalePercent
        ));
        await _db.SaveChangesAsync();
        return channel;
    }

    public async Task<bool> DeleteChannelAsync(Guid id)
    {
        var channel = await _db.Channels.FindAsync(id);
        if (channel is null) return false;

        _db.Channels.Remove(channel);
        await _db.SaveChangesAsync();
        return true;
    }

    // ===== Release =====

    public async Task<List<SoftwareRelease>> GetReleasesBySoftwareIdAsync(Guid softwareId, Guid? channelId)
    {
        var query = _db.SoftwareReleases.Where(r => r.SoftwareId == softwareId);
        if (channelId.HasValue)
            query = query.Where(r => r.ChannelId == channelId.Value);

        return await query.OrderByDescending(r => r.ReleaseTime).ToListAsync();
    }

    public async Task<SoftwareRelease?> GetReleaseByIdAsync(Guid id)
    {
        return await _db.SoftwareReleases.FindAsync(id);
    }

    public async Task<SoftwareRelease> CreateReleaseAsync(CreateReleaseRequest request)
    {
        var release = new SoftwareRelease(
            Guid.NewGuid(),
            request.SoftwareId,
            request.ChannelId,
            request.Version,
            request.UpdateLog,
            request.FilePath,
            request.FileSize,
            request.FileHash,
            request.IsForceUpdate,
            DateTime.UtcNow,
            request.IsOnline
        );
        _db.SoftwareReleases.Add(release);
        await _db.SaveChangesAsync();
        return release;
    }

    public async Task<SoftwareRelease?> UpdateReleaseAsync(Guid id, UpdateReleaseRequest request)
    {
        var release = await _db.SoftwareReleases.FindAsync(id);
        if (release is null) return null;

        _db.SoftwareReleases.Entry(release).CurrentValues.SetValues(new SoftwareRelease(
            id,
            release.SoftwareId,
            request.ChannelId,
            request.Version,
            request.UpdateLog,
            request.FilePath,
            request.FileSize,
            request.FileHash,
            request.IsForceUpdate,
            release.ReleaseTime,
            request.IsOnline
        ));
        await _db.SaveChangesAsync();
        return release;
    }

    public async Task<bool> DeleteReleaseAsync(Guid id)
    {
        var release = await _db.SoftwareReleases.FindAsync(id);
        if (release is null) return false;

        _db.SoftwareReleases.Remove(release);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<SoftwareRelease?> ToggleReleaseOnlineAsync(Guid id)
    {
        var release = await _db.SoftwareReleases.FindAsync(id);
        if (release is null) return null;

        _db.SoftwareReleases.Entry(release).CurrentValues.SetValues(new SoftwareRelease(
            id,
            release.SoftwareId,
            release.ChannelId,
            release.Version,
            release.UpdateLog,
            release.FilePath,
            release.FileSize,
            release.FileHash,
            release.IsForceUpdate,
            release.ReleaseTime,
            !release.IsOnline
        ));
        await _db.SaveChangesAsync();
        return release;
    }

    // ===== Client Check Update =====

    public async Task<CheckUpdateResponse?> CheckUpdateAsync(
        string appKey, int channelCode, Version currentVersion, string? deviceId)
    {
        var software = await _db.Software.FirstOrDefaultAsync(s => s.AppKey == appKey);
        if (software is null) return null;

        var channel = await _db.Channels
            .FirstOrDefaultAsync(c => c.SoftwareId == software.SoftwareId && c.ChannelCode == channelCode);
        if (channel is null) return null;

        var latestRelease = await _db.SoftwareReleases
            .Where(r => r.SoftwareId == software.SoftwareId
                     && r.ChannelId == channel.ChannelId
                     && r.IsOnline)
            .OrderByDescending(r => r.ReleaseTime)
            .FirstOrDefaultAsync();

        if (latestRelease is null)
            return new CheckUpdateResponse(false, null, null, null, null, null, false);

        if (latestRelease.Version <= currentVersion)
            return new CheckUpdateResponse(false, null, null, null, null, null, false);

        if (channel.GrayScalePercent < 100 && !string.IsNullOrEmpty(deviceId))
        {
            var hash = Math.Abs(deviceId.GetHashCode()) % 100;
            if (hash >= channel.GrayScalePercent)
                return new CheckUpdateResponse(false, null, null, null, null, null, false);
        }

        return new CheckUpdateResponse(
            true,
            latestRelease.Version,
            latestRelease.UpdateLog,
            latestRelease.FilePath,
            latestRelease.FileSize,
            latestRelease.FileHash,
            latestRelease.IsForceUpdate
        );
    }
}
