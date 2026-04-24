using System.Text.Json;
using EchoHub.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

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
        var stringListConverter = new ValueConverter<List<string>, string>(
            v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
            v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>());

        var stringListComparer = new ValueComparer<List<string>>(
            (a, b) => (a == null && b == null) || (a != null && b != null && a.SequenceEqual(b)),
            c => c == null ? 0 : c.Aggregate(0, (h, v) => HashCode.Combine(h, v.GetHashCode())),
            c => c == null ? new List<string>() : c.ToList());

        modelBuilder.Entity<Server>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Version).IsRequired().HasMaxLength(50);

            entity.Property(e => e.Hosts)
                .HasColumnType("jsonb")
                .HasConversion(stringListConverter, stringListComparer);

            entity.Property(e => e.Tags)
                .HasColumnType("jsonb")
                .HasConversion(stringListConverter, stringListComparer);

            entity.HasIndex(e => e.Hosts).HasMethod("gin");
            entity.HasIndex(e => e.Tags).HasMethod("gin");
        });
    }
}
