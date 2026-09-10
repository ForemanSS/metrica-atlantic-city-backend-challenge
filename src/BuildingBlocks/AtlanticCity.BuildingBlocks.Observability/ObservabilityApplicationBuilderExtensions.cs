using Microsoft.AspNetCore.Builder;

namespace AtlanticCity.BuildingBlocks.Observability;

public static class
    ObservabilityApplicationBuilderExtensions
{
    public static IApplicationBuilder
        UseAtlanticCityCorrelationId(
            this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(
            app);

        return app.UseMiddleware<
            CorrelationIdMiddleware>();
    }

    public static IApplicationBuilder
        UseAtlanticCityExceptionHandling(
            this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(
            app);

        return app.UseExceptionHandler();
    }
}