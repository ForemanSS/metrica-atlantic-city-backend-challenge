using Microsoft.Extensions.DependencyInjection;

namespace AtlanticCity.Authentication.Infrastructure.Initialization;

public static class AuthenticationInitializationExtensions
{
    public static async Task InitializeAuthenticationAsync(
        this IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        await using var scope =
            serviceProvider.CreateAsyncScope();

        var initializer =
            scope.ServiceProvider
                .GetRequiredService<
                    AuthenticationDatabaseInitializer>();

        await initializer.InitializeAsync(
            cancellationToken);
    }
}