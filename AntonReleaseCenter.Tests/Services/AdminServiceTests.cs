namespace AntonReleaseCenter.Tests.Services;

public class AdminServiceTests
{
    private static AppDbContext CreateDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new AppDbContext(options);
    }

    private static AdminService CreateService(AppDbContext db) => new(db);

    [Fact]
    public async Task GetAllAdminsAsync_EmptyDb_ReturnsEmptyList()
    {
        var db = CreateDbContext(nameof(GetAllAdminsAsync_EmptyDb_ReturnsEmptyList));
        var service = CreateService(db);

        var result = await service.GetAllAdminsAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllAdminsAsync_MultipleAdmins_ReturnsAll()
    {
        var db = CreateDbContext(nameof(GetAllAdminsAsync_MultipleAdmins_ReturnsAll));
        var service = CreateService(db);
        await service.CreateAdminAsync(new CreateAdminRequest("admin1", "hash1"));
        await service.CreateAdminAsync(new CreateAdminRequest("admin2", "hash2"));

        var result = await service.GetAllAdminsAsync();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task CreateAdminAsync_ValidRequest_ReturnsAdminResponse()
    {
        var db = CreateDbContext(nameof(CreateAdminAsync_ValidRequest_ReturnsAdminResponse));
        var service = CreateService(db);
        var request = new CreateAdminRequest("admin", "passwordhash");

        var result = await service.CreateAdminAsync(request);

        Assert.NotEqual(Guid.Empty, result.AdminId);
        Assert.Equal("admin", result.Username);
    }

    [Fact]
    public async Task CreateAdminAsync_ValidRequest_SavesPasswordHash()
    {
        var db = CreateDbContext(nameof(CreateAdminAsync_ValidRequest_SavesPasswordHash));
        var service = CreateService(db);
        var request = new CreateAdminRequest("admin", "secret_hash");

        await service.CreateAdminAsync(request);

        var saved = await db.Admins.FirstOrDefaultAsync(a => a.Username == "admin");
        Assert.NotNull(saved);
        Assert.Equal("secret_hash", saved.PasswordHash);
    }

    [Fact]
    public async Task GetAdminByIdAsync_ExistingId_ReturnsAdminResponse()
    {
        var db = CreateDbContext(nameof(GetAdminByIdAsync_ExistingId_ReturnsAdminResponse));
        var service = CreateService(db);
        var created = await service.CreateAdminAsync(new CreateAdminRequest("admin", "hash"));

        var result = await service.GetAdminByIdAsync(created.AdminId);

        Assert.NotNull(result);
        Assert.Equal(created.AdminId, result.AdminId);
        Assert.Equal("admin", result.Username);
    }

    [Fact]
    public async Task GetAdminByIdAsync_NonExistingId_ReturnsNull()
    {
        var db = CreateDbContext(nameof(GetAdminByIdAsync_NonExistingId_ReturnsNull));
        var service = CreateService(db);

        var result = await service.GetAdminByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAdminAsync_ExistingId_UpdatesFields()
    {
        var db = CreateDbContext(nameof(UpdateAdminAsync_ExistingId_UpdatesFields));
        var service = CreateService(db);
        var created = await service.CreateAdminAsync(new CreateAdminRequest("old_name", "old_hash"));

        var result = await service.UpdateAdminAsync(created.AdminId,
            new UpdateAdminRequest("new_name", "new_hash"));

        Assert.NotNull(result);
        Assert.Equal("new_name", result.Username);
        Assert.Equal("new_hash", (await db.Admins.FindAsync(created.AdminId))!.PasswordHash);
    }

    [Fact]
    public async Task UpdateAdminAsync_NonExistingId_ReturnsNull()
    {
        var db = CreateDbContext(nameof(UpdateAdminAsync_NonExistingId_ReturnsNull));
        var service = CreateService(db);

        var result = await service.UpdateAdminAsync(Guid.NewGuid(),
            new UpdateAdminRequest("name", "hash"));

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAdminAsync_ExistingId_ReturnsTrue()
    {
        var db = CreateDbContext(nameof(DeleteAdminAsync_ExistingId_ReturnsTrue));
        var service = CreateService(db);
        var created = await service.CreateAdminAsync(new CreateAdminRequest("admin", "hash"));

        var deleted = await service.DeleteAdminAsync(created.AdminId);

        Assert.True(deleted);
        Assert.Null(await service.GetAdminByIdAsync(created.AdminId));
    }

    [Fact]
    public async Task DeleteAdminAsync_NonExistingId_ReturnsFalse()
    {
        var db = CreateDbContext(nameof(DeleteAdminAsync_NonExistingId_ReturnsFalse));
        var service = CreateService(db);

        var deleted = await service.DeleteAdminAsync(Guid.NewGuid());

        Assert.False(deleted);
    }
}
