using EchoHub.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace EchoHub.Infrastructure.Data;

/// <summary>
/// Entity Framework Core database context for the EchoHub application.
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    /// <summary>
    /// Gets the set of registered servers.
    /// </summary>
    public DbSet<Server> Servers => Set<Server>();

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Server>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Host).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Description).HasMaxLength(500);
        });
    }
}
