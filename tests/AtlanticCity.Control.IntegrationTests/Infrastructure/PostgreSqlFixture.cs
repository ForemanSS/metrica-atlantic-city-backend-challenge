using AtlanticCity.Control.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace AtlanticCity.Control.IntegrationTests.Infrastructure;

public sealed class PostgreSqlFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container =
        new PostgreSqlBuilder("postgres:17-alpine")
            .WithDatabase("atlanticcity_tests")
            .WithUsername("atlanticcity")
            .WithPassword("AtlanticCity_Test_2026!")
            .Build();

    public string ConnectionString =>
        _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        var options =
            new DbContextOptionsBuilder<ControlDbContext>()
                .UseNpgsql(ConnectionString)
                .Options;

        await using var dbContext =
            new ControlDbContext(options);

        await dbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }

    public ControlDbContext CreateDbContext()
    {
        var options =
            new DbContextOptionsBuilder<ControlDbContext>()
                .UseNpgsql(ConnectionString)
                .Options;

        return new ControlDbContext(options);
    }
}