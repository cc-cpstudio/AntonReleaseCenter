using AntonReleaseCenter.Core.Models;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace AntonReleaseCenter.Server.DbContexts;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Admin> Admins { get; set; }
    public DbSet<Channel> Channels { get; set; }
    public DbSet<Software> Software { get; set; }
    public DbSet<SoftwareRelease> SoftwareReleases { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var versionConverter = new ValueConverter<Core.Models.Version, string>(
            v => v.ToString(),
            s => Core.Models.Version.Parse(s)
        );

        modelBuilder.Entity<SoftwareRelease>()
            .Property(r => r.Version)
            .HasConversion(versionConverter);

        modelBuilder.Entity<Channel>()
            .HasOne<Software>()
            .WithMany()
            .HasForeignKey(c => c.SoftwareId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SoftwareRelease>()
            .HasOne<Software>()
            .WithMany()
            .HasForeignKey(r => r.SoftwareId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<SoftwareRelease>()
            .HasOne<Channel>()
            .WithMany()
            .HasForeignKey(r => r.ChannelId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
