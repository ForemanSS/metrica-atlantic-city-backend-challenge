using Microsoft.Extensions.DependencyInjection;

namespace AtlanticCity.BuildingBlocks.Observability;

public static class
    ObservabilityServiceCollectionExtensions
{
    public static IServiceCollection
        AddAtlanticCityProblemDetails(
            this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(
            services);

        services.AddProblemDetails();

        services.AddExceptionHandler<
            GlobalExceptionHandler>();

        return services;
    }
}