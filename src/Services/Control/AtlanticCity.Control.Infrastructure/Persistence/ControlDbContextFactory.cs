using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AtlanticCity.Control.Infrastructure.Persistence;

public sealed class ControlDbContextFactory
    : IDesignTimeDbContextFactory<ControlDbContext>
{
    public ControlDbContext CreateDbContext(
        string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable(
                "ConnectionStrings__ControlDb")
            ??
            "Host=localhost;Port=5432;Database=atlanticcity;Username=atlanticcity;Password=atlanticcity_dev_2026";

        var optionsBuilder =
            new DbContextOptionsBuilder<
                ControlDbContext>();

        optionsBuilder.UseNpgsql(
            connectionString);

        return new ControlDbContext(
            optionsBuilder.Options);
    }
}