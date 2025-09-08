using Microsoft.EntityFrameworkCore;
using SsoBackend.Domain;

namespace SsoBackend.Infrastructure;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    // Add other DbSets as needed
}
