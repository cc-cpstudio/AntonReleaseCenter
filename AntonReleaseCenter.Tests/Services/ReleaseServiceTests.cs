namespace AntonReleaseCenter.Tests.Services;

public class ReleaseServiceTests
{
    private static AppDbContext CreateDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new AppDbContext(options);
    }

    private static ReleaseService CreateService(AppDbContext db) => new(db);

    // ===== Software Tests =====

    [Fact]
    public async Task GetAllSoftwareAsync_EmptyDb_ReturnsEmptyList()
    {
        var db = CreateDbContext(nameof(GetAllSoftwareAsync_EmptyDb_ReturnsEmptyList));
        var service = CreateService(db);

        var result = await service.GetAllSoftwareAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task CreateSoftwareAsync_ValidRequest_ReturnsSoftware()
    {
        var db = CreateDbContext(nameof(CreateSoftwareAsync_ValidRequest_ReturnsSoftware));
        var service = CreateService(db);
        var request = new CreateSoftwareRequest("test-key", "TestApp", "description", true);

        var result = await service.CreateSoftwareAsync(request);

        Assert.NotEqual(Guid.Empty, result.SoftwareId);
        Assert.Equal("test-key", result.AppKey);
        Assert.Equal("TestApp", result.Name);
        Assert.True(result.IsEnabled);
    }

    [Fact]
    public async Task GetSoftwareByIdAsync_ExistingId_ReturnsSoftware()
    {
        var db = CreateDbContext(nameof(GetSoftwareByIdAsync_ExistingId_ReturnsSoftware));
        var service = CreateService(db);
        var created = await service.CreateSoftwareAsync(new CreateSoftwareRequest("key", "App", "desc", true));

        var result = await service.GetSoftwareByIdAsync(created.SoftwareId);

        Assert.NotNull(result);
        Assert.Equal(created.SoftwareId, result.SoftwareId);
    }

    [Fact]
    public async Task GetSoftwareByIdAsync_NonExistingId_ReturnsNull()
    {
        var db = CreateDbContext(nameof(GetSoftwareByIdAsync_NonExistingId_ReturnsNull));
        var service = CreateService(db);

        var result = await service.GetSoftwareByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateSoftwareAsync_ExistingId_UpdatesFields()
    {
        var db = CreateDbContext(nameof(UpdateSoftwareAsync_ExistingId_UpdatesFields));
        var service = CreateService(db);
        var created = await service.CreateSoftwareAsync(new CreateSoftwareRequest("key", "Old", "desc", true));

        var result = await service.UpdateSoftwareAsync(created.SoftwareId,
            new UpdateSoftwareRequest("NewName", "new desc", false));

        Assert.NotNull(result);
        Assert.Equal("NewName", result.Name);
        Assert.False(result.IsEnabled);
    }

    [Fact]
    public async Task UpdateSoftwareAsync_NonExistingId_ReturnsNull()
    {
        var db = CreateDbContext(nameof(UpdateSoftwareAsync_NonExistingId_ReturnsNull));
        var service = CreateService(db);

        var result = await service.UpdateSoftwareAsync(Guid.NewGuid(),
            new UpdateSoftwareRequest("Name", "desc", true));

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteSoftwareAsync_ExistingId_ReturnsTrue()
    {
        var db = CreateDbContext(nameof(DeleteSoftwareAsync_ExistingId_ReturnsTrue));
        var service = CreateService(db);
        var created = await service.CreateSoftwareAsync(new CreateSoftwareRequest("key", "App", "desc", true));

        var deleted = await service.DeleteSoftwareAsync(created.SoftwareId);

        Assert.True(deleted);
        Assert.Null(await service.GetSoftwareByIdAsync(created.SoftwareId));
    }

    [Fact]
    public async Task DeleteSoftwareAsync_NonExistingId_ReturnsFalse()
    {
        var db = CreateDbContext(nameof(DeleteSoftwareAsync_NonExistingId_ReturnsFalse));
        var service = CreateService(db);

        var deleted = await service.DeleteSoftwareAsync(Guid.NewGuid());

        Assert.False(deleted);
    }

    // ===== Channel Tests =====

    [Fact]
    public async Task CreateChannelAsync_ValidRequest_ReturnsChannel()
    {
        var db = CreateDbContext(nameof(CreateChannelAsync_ValidRequest_ReturnsChannel));
        var service = CreateService(db);
        var software = await service.CreateSoftwareAsync(new CreateSoftwareRequest("key", "App", "desc", true));

        var channel = await service.CreateChannelAsync(software.SoftwareId,
            new CreateChannelRequest(1, "Stable", 100));

        Assert.NotEqual(Guid.Empty, channel.ChannelId);
        Assert.Equal(software.SoftwareId, channel.SoftwareId);
        Assert.Equal(1, channel.ChannelCode);
        Assert.Equal("Stable", channel.ChannelName);
    }

    [Fact]
    public async Task GetChannelsBySoftwareIdAsync_ReturnsOnlyMatching()
    {
        var db = CreateDbContext(nameof(GetChannelsBySoftwareIdAsync_ReturnsOnlyMatching));
        var service = CreateService(db);
        var s1 = await service.CreateSoftwareAsync(new CreateSoftwareRequest("k1", "A", "d", true));
        var s2 = await service.CreateSoftwareAsync(new CreateSoftwareRequest("k2", "B", "d", true));
        await service.CreateChannelAsync(s1.SoftwareId, new CreateChannelRequest(1, "Stable", 100));
        await service.CreateChannelAsync(s2.SoftwareId, new CreateChannelRequest(2, "Beta", 50));

        var result = await service.GetChannelsBySoftwareIdAsync(s1.SoftwareId);

        Assert.Single(result);
        Assert.Equal("Stable", result[0].ChannelName);
    }

    // ===== Release Tests =====

    [Fact]
    public async Task CreateReleaseAsync_ValidRequest_ReturnsRelease()
    {
        var db = CreateDbContext(nameof(CreateReleaseAsync_ValidRequest_ReturnsRelease));
        var service = CreateService(db);
        var software = await service.CreateSoftwareAsync(new CreateSoftwareRequest("key", "App", "desc", true));
        var channel = await service.CreateChannelAsync(software.SoftwareId,
            new CreateChannelRequest(1, "Stable", 100));
        var version = new Version(1, 0, 0, 0);

        var release = await service.CreateReleaseAsync(new CreateReleaseRequest(
            software.SoftwareId, channel.ChannelId, PlatformEnum.Windows_x64, version,
            "First release", "/path/file.zip", 1024, "abc123",
            false, true));

        Assert.NotEqual(Guid.Empty, release.SoftwareReleaseId);
        Assert.Equal(version, release.Version);
        Assert.Equal("First release", release.UpdateLog);
        Assert.True(release.IsOnline);
    }

    [Fact]
    public async Task ToggleReleaseOnlineAsync_FlipsStatus()
    {
        var db = CreateDbContext(nameof(ToggleReleaseOnlineAsync_FlipsStatus));
        var service = CreateService(db);
        var software = await service.CreateSoftwareAsync(new CreateSoftwareRequest("key", "App", "desc", true));
        var channel = await service.CreateChannelAsync(software.SoftwareId,
            new CreateChannelRequest(1, "Stable", 100));
        var release = await service.CreateReleaseAsync(new CreateReleaseRequest(
            software.SoftwareId, channel.ChannelId, PlatformEnum.Windows_x64, new Version(1, 0, 0, 0),
            "log", "/path.zip", 1024, "hash", false, true));

        var toggled = await service.ToggleReleaseOnlineAsync(release.SoftwareReleaseId);

        Assert.NotNull(toggled);
        Assert.False(toggled.IsOnline);

        var toggledBack = await service.ToggleReleaseOnlineAsync(release.SoftwareReleaseId);
        Assert.NotNull(toggledBack);
        Assert.True(toggledBack.IsOnline);
    }

    [Fact]
    public async Task GetReleasesBySoftwareNameAndPlatformAsync_ReturnsFiltered()
    {
        var db = CreateDbContext(nameof(GetReleasesBySoftwareNameAndPlatformAsync_ReturnsFiltered));
        var service = CreateService(db);
        var software = await service.CreateSoftwareAsync(new CreateSoftwareRequest("myapp", "MyApp", "desc", true));
        var channel = await service.CreateChannelAsync(software.SoftwareId,
            new CreateChannelRequest(1, "Stable", 100));
        await service.CreateReleaseAsync(new CreateReleaseRequest(
            software.SoftwareId, channel.ChannelId, PlatformEnum.Windows_x64, new Version(1, 0, 0, 0),
            "win64", "/path.zip", 1024, "hash", false, true));
        await service.CreateReleaseAsync(new CreateReleaseRequest(
            software.SoftwareId, channel.ChannelId, PlatformEnum.MacOS_AppleSilicon, new Version(1, 0, 0, 0),
            "mac", "/path.zip", 1024, "hash", false, true));

        var result = await service.GetReleasesBySoftwareNameAndPlatformAsync("myapp", PlatformEnum.Windows_x64);

        Assert.Single(result);
        Assert.Equal(PlatformEnum.Windows_x64, result[0].Platform);
    }

    // ===== Check Update Tests =====

    [Fact]
    public async Task CheckUpdateAsync_SoftwareNotFound_ReturnsNull()
    {
        var db = CreateDbContext(nameof(CheckUpdateAsync_SoftwareNotFound_ReturnsNull));
        var service = CreateService(db);

        var result = await service.CheckUpdateAsync("non-existent", 1, new Version(1, 0, 0, 0), null);

        Assert.Null(result);
    }

    [Fact]
    public async Task CheckUpdateAsync_ChannelNotFound_ReturnsNull()
    {
        var db = CreateDbContext(nameof(CheckUpdateAsync_ChannelNotFound_ReturnsNull));
        var service = CreateService(db);
        await service.CreateSoftwareAsync(new CreateSoftwareRequest("key", "App", "desc", true));

        var result = await service.CheckUpdateAsync("key", 999, new Version(1, 0, 0, 0), null);

        Assert.Null(result);
    }

    [Fact]
    public async Task CheckUpdateAsync_NoOnlineRelease_ReturnsNoUpdate()
    {
        var db = CreateDbContext(nameof(CheckUpdateAsync_NoOnlineRelease_ReturnsNoUpdate));
        var service = CreateService(db);
        var software = await service.CreateSoftwareAsync(new CreateSoftwareRequest("key", "App", "desc", true));
        var channel = await service.CreateChannelAsync(software.SoftwareId,
            new CreateChannelRequest(1, "Stable", 100));
        await service.CreateReleaseAsync(new CreateReleaseRequest(
            software.SoftwareId, channel.ChannelId, PlatformEnum.Windows_x64, new Version(2, 0, 0, 0),
            "offline", "/path.zip", 1024, "hash", false, false));

        var result = await service.CheckUpdateAsync("key", 1, new Version(1, 0, 0, 0), null);

        Assert.NotNull(result);
        Assert.False(result.HasUpdate);
    }

    [Fact]
    public async Task CheckUpdateAsync_CurrentVersionIsLatest_ReturnsNoUpdate()
    {
        var db = CreateDbContext(nameof(CheckUpdateAsync_CurrentVersionIsLatest_ReturnsNoUpdate));
        var service = CreateService(db);
        var software = await service.CreateSoftwareAsync(new CreateSoftwareRequest("key", "App", "desc", true));
        var channel = await service.CreateChannelAsync(software.SoftwareId,
            new CreateChannelRequest(1, "Stable", 100));
        await service.CreateReleaseAsync(new CreateReleaseRequest(
            software.SoftwareId, channel.ChannelId, PlatformEnum.Windows_x64, new Version(1, 0, 0, 0),
            "latest", "/path.zip", 1024, "hash", false, true));

        var result = await service.CheckUpdateAsync("key", 1, new Version(1, 0, 0, 0), null);

        Assert.NotNull(result);
        Assert.False(result.HasUpdate);
    }

    [Fact]
    public async Task CheckUpdateAsync_NewerVersionAvailable_ReturnsUpdate()
    {
        var db = CreateDbContext(nameof(CheckUpdateAsync_NewerVersionAvailable_ReturnsUpdate));
        var service = CreateService(db);
        var software = await service.CreateSoftwareAsync(new CreateSoftwareRequest("key", "App", "desc", true));
        var channel = await service.CreateChannelAsync(software.SoftwareId,
            new CreateChannelRequest(1, "Stable", 100));
        await service.CreateReleaseAsync(new CreateReleaseRequest(
            software.SoftwareId, channel.ChannelId, PlatformEnum.Windows_x64, new Version(2, 0, 0, 0),
            "New version!", "/path/v2.zip", 2048, "hash2", true, true));

        var result = await service.CheckUpdateAsync("key", 1, new Version(1, 0, 0, 0), null);

        Assert.NotNull(result);
        Assert.True(result.HasUpdate);
        Assert.Equal("New version!", result.UpdateLog);
        Assert.Equal("/path/v2.zip", result.DownloadUrl);
        Assert.True(result.IsForceUpdate);
    }

    [Fact]
    public async Task CheckUpdateAsync_GrayScale_DeviceInRange_GetsUpdate()
    {
        var db = CreateDbContext(nameof(CheckUpdateAsync_GrayScale_DeviceInRange_GetsUpdate));
        var service = CreateService(db);
        var software = await service.CreateSoftwareAsync(new CreateSoftwareRequest("key", "App", "desc", true));
        var channel = await service.CreateChannelAsync(software.SoftwareId,
            new CreateChannelRequest(1, "Beta", 50));
        await service.CreateReleaseAsync(new CreateReleaseRequest(
            software.SoftwareId, channel.ChannelId, PlatformEnum.Windows_x64, new Version(2, 0, 0, 0),
            "beta", "/path.zip", 1024, "hash", false, true));

        var deviceId = "device-in-range";
        var result = await service.CheckUpdateAsync("key", 1, new Version(1, 0, 0, 0), deviceId);

        var hash = Math.Abs(deviceId.GetHashCode()) % 100;
        if (hash < 50)
        {
            Assert.True(result!.HasUpdate, $"deviceId '{deviceId}' hash={hash} should be < 50");
        }
        else
        {
            Assert.False(result!.HasUpdate, $"deviceId '{deviceId}' hash={hash} should be >= 50");
        }
    }

    [Fact]
    public async Task CheckUpdateAsync_ForceUpdate_ReturnsForceFlag()
    {
        var db = CreateDbContext(nameof(CheckUpdateAsync_ForceUpdate_ReturnsForceFlag));
        var service = CreateService(db);
        var software = await service.CreateSoftwareAsync(new CreateSoftwareRequest("key", "App", "desc", true));
        var channel = await service.CreateChannelAsync(software.SoftwareId,
            new CreateChannelRequest(1, "Stable", 100));
        await service.CreateReleaseAsync(new CreateReleaseRequest(
            software.SoftwareId, channel.ChannelId, PlatformEnum.Windows_x64, new Version(3, 0, 0, 0),
            "forced update", "/path.zip", 1024, "hash", true, true));

        var result = await service.CheckUpdateAsync("key", 1, new Version(1, 0, 0, 0), null);

        Assert.NotNull(result);
        Assert.True(result.IsForceUpdate);
    }
}
