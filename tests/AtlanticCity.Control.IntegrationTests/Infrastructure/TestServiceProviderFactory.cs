using AtlanticCity.Control.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace AtlanticCity.Control.IntegrationTests.Infrastructure;

internal static class TestServiceProviderFactory
{
    public static ServiceProvider Create(
        string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            connectionString);

        var services =
            new ServiceCollection();

        services.AddControlPersistence(
            connectionString);

        return services.BuildServiceProvider(
            new ServiceProviderOptions
            {
                ValidateScopes = true,
                ValidateOnBuild = true
            });
    }
}