namespace AntonReleaseCenter.Server.Services;

public class AdminService : IAdminService
{
    private readonly AppDbContext _db;

    public AdminService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<AdminResponse>> GetAllAdminsAsync()
    {
        return await _db.Admins
            .Select(a => new AdminResponse(a.AdminId, a.Username))
            .ToListAsync();
    }

    public async Task<AdminResponse?> GetAdminByIdAsync(Guid id)
    {
        var admin = await _db.Admins.FindAsync(id);
        if (admin is null) return null;
        return new AdminResponse(admin.AdminId, admin.Username);
    }

    public async Task<AdminResponse> CreateAdminAsync(CreateAdminRequest request)
    {
        var admin = new Admin(
            Guid.NewGuid(),
            request.Username,
            request.PasswordHash
        );
        _db.Admins.Add(admin);
        await _db.SaveChangesAsync();
        return new AdminResponse(admin.AdminId, admin.Username);
    }

    public async Task<AdminResponse?> UpdateAdminAsync(Guid id, UpdateAdminRequest request)
    {
        var admin = await _db.Admins.FindAsync(id);
        if (admin is null) return null;

        _db.Admins.Entry(admin).CurrentValues.SetValues(new Admin(
            id,
            request.Username,
            request.PasswordHash
        ));
        await _db.SaveChangesAsync();
        return new AdminResponse(admin.AdminId, admin.Username);
    }

    public async Task<bool> DeleteAdminAsync(Guid id)
    {
        var admin = await _db.Admins.FindAsync(id);
        if (admin is null) return false;

        _db.Admins.Remove(admin);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ChangePasswordAsync(Guid adminId, string oldPasswordHash, string newPasswordHash)
    {
        var admin = await _db.Admins.FindAsync(adminId);
        if (admin is null) return false;
        if (admin.PasswordHash != oldPasswordHash) return false;

        _db.Admins.Entry(admin).CurrentValues.SetValues(new Admin(
            adminId,
            admin.Username,
            newPasswordHash
        ));
        await _db.SaveChangesAsync();
        return true;
    }
}
