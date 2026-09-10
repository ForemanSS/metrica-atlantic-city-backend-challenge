using AtlanticCity.MassiveLoad.Application.Processing;
using Microsoft.Extensions.DependencyInjection;

namespace AtlanticCity.MassiveLoad.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddMassiveLoadApplication(
        this IServiceCollection services)
    {
        services.AddScoped<
            ProcessMassiveLoadCommandHandler>();

        return services;
    }
}