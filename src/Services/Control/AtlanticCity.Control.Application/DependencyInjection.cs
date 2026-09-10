using AtlanticCity.Control.Application.Loads.Upload;
using Microsoft.Extensions.DependencyInjection;
using AtlanticCity.Control.Application.Loads.Queries.Detail;
using AtlanticCity.Control.Application.Loads.Queries.List;
using AtlanticCity.Control.Application.Loads.Queries.Data;
using AtlanticCity.Control.Application.Loads.Queries.Errors;
using AtlanticCity.Control.Application.Loads.Queries.History;

namespace AtlanticCity.Control.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddControlApplication(
        this IServiceCollection services,
        LoadUploadSettings? uploadSettings = null)
    {
        services.AddScoped<CreateLoadCommandHandler>();

        services.AddScoped<
            ListLoadsQueryHandler>();

        services.AddScoped<
            GetLoadDetailQueryHandler>();

        services.AddScoped<
            GetLoadHistoryQueryHandler>();

        services.AddScoped<
            GetLoadDataQueryHandler>();

        services.AddScoped<
            GetLoadErrorsQueryHandler>();

        services.AddSingleton(
            uploadSettings ??
            LoadUploadSettings.Default);

        return services;
    }
}