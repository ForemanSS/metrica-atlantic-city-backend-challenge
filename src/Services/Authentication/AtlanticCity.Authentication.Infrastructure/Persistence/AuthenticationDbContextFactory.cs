using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AtlanticCity.Authentication.Infrastructure.Persistence;

public sealed class AuthenticationDbContextFactory
    : IDesignTimeDbContextFactory<AuthenticationDbContext>
{
    public AuthenticationDbContext CreateDbContext(
        string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable(
                "ConnectionStrings__AuthenticationDb")
            ??
            "Host=localhost;Port=5432;Database=atlanticcity_auth;Username=atlanticcity;Password=atlanticcity_dev_2026";

        var optionsBuilder =
            new DbContextOptionsBuilder<
                AuthenticationDbContext>();

        optionsBuilder.UseNpgsql(
            connectionString);

        return new AuthenticationDbContext(
            optionsBuilder.Options);
    }
}