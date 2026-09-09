using AtlanticCity.Control.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace AtlanticCity.Control.IntegrationTests.Infrastructure;

internal static class TestServiceProviderFactory
{
    public static ServiceProvider Create(
        string connectionString)
    {
        var services = new ServiceCollection();

        services.AddLogging();

        services.AddControlInfrastructure(
            connectionString);

        return services.BuildServiceProvider();
    }
}