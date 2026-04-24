using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EchoHub.Infrastructure.Data;

/// <summary>
/// Design-time factory used by the EF Core tooling (migrations/scaffolding) to build an <see cref="AppDbContext"/>
/// configured against Npgsql. The runtime app uses a different provider (currently InMemory).
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    /// <inheritdoc />
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql("Host=localhost;Database=echohub;Username=echohub;Password=echohub")
            .Options;

        return new AppDbContext(options);
    }
}
