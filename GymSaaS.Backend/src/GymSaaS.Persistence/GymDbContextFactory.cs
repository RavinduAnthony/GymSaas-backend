using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GymSaaS.Persistence;

/// <summary>
/// Design-time factory used by EF Core tooling (dotnet ef migrations add / update)
/// when the application host cannot be constructed.
/// </summary>
public class GymDbContextFactory : IDesignTimeDbContextFactory<GymDbContext>
{
    public GymDbContext CreateDbContext(string[] args)
    {
        // Must match Program.cs to keep timestamp column types consistent
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        var connectionString = "Host=localhost;Port=5432;Database=GymSaaSDb;Username=postgres;Password=myPostgre@123";

        var optionsBuilder = new DbContextOptionsBuilder<GymDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new GymDbContext(optionsBuilder.Options, new TenantProvider());
    }
}
