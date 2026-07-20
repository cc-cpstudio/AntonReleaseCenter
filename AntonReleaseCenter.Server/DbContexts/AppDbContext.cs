using AntonReleaseCenter.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace AntonReleaseCenter.Server.DbContexts;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Admin> Admins { get; set; }
    public DbSet<Channel> Channels { get; set; }
    public DbSet<Software> Software { get; set; }
    public DbSet<SoftwareRelease>  SoftwareReleases { get; set; }
}