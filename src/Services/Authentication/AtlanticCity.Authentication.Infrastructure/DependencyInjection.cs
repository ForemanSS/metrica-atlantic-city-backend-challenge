using AtlanticCity.Authentication.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AtlanticCity.Authentication.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAuthenticationInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            connectionString);

        services.AddDbContext<AuthenticationDbContext>(
            options =>
                options.UseNpgsql(connectionString));

        return services;
    }
}