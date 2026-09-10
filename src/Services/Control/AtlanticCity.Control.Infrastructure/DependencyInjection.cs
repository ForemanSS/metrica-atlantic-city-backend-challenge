using AtlanticCity.Control.Application.Loads.PeriodReservation;
using AtlanticCity.Control.Application.Loads.Persistence;
using AtlanticCity.Control.Application.Messaging;
using AtlanticCity.Control.Application.Storage;
using AtlanticCity.Control.Infrastructure.Messaging;
using AtlanticCity.Control.Infrastructure.Persistence;
using AtlanticCity.Control.Infrastructure.Persistence.PeriodReservation;
using AtlanticCity.Control.Infrastructure.Persistence.Repositories;
using AtlanticCity.Control.Infrastructure.Storage;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using AtlanticCity.Control.Application.Loads.Queries;
using AtlanticCity.Control.Infrastructure.Persistence.Queries;

namespace AtlanticCity.Control.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddControlInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString(
                "ControlDb")
            ?? throw new InvalidOperationException(
                "Connection string 'ControlDb' was not configured.");

        services.AddControlPersistence(
            connectionString);

        ConfigureSeaweedFs(
            services,
            configuration);

        ConfigureMessaging(
            services,
            configuration);

        return services;
    }

    public static IServiceCollection AddControlPersistence(
        this IServiceCollection services,
        string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            connectionString);

        services.AddLogging();

        services.AddDbContext<ControlDbContext>(
            options =>
                options.UseNpgsql(
                    connectionString));

        services.AddSingleton(
            _ =>
                NpgsqlDataSource.Create(
                    connectionString));

        services.AddScoped<
            ILoadRepository,
            LoadRepository>();

        services.AddScoped<
            ILoadQueryStore,
            LoadQueryStore>();

        services.AddScoped<
            ILoadPeriodReservationStore,
            LoadPeriodReservationStore>();

        return services;
    }

    private static void ConfigureSeaweedFs(
        IServiceCollection services,
        IConfiguration configuration)
    {
        var baseAddress =
            configuration[
                "SeaweedFs:BaseAddress"]
            ?? "http://localhost:8888/";

        var rootPath =
            configuration[
                "SeaweedFs:RootPath"]
            ?? "loads";

        if (!Uri.TryCreate(
                baseAddress,
                UriKind.Absolute,
                out var baseUri))
        {
            throw new InvalidOperationException(
                "SeaweedFs:BaseAddress is invalid.");
        }

        var options =
            new SeaweedFileStorageOptions(
                baseUri.ToString(),
                rootPath);

        services.AddSingleton(
            options);

        services.AddHttpClient<
                ILoadFileStorage,
                SeaweedFileStorage>(
                client =>
                {
                    client.BaseAddress =
                        baseUri;

                    client.Timeout =
                        TimeSpan.FromMinutes(2);
                });
    }

    private static void ConfigureMessaging(
        IServiceCollection services,
        IConfiguration configuration)
    {
        var host =
            configuration[
                "RabbitMq:Host"]
            ?? "localhost";

        var virtualHost =
            configuration[
                "RabbitMq:VirtualHost"]
            ?? "/";

        var username =
            configuration[
                "RabbitMq:Username"]
            ?? throw new InvalidOperationException(
                "RabbitMq:Username was not configured.");

        var password =
            configuration[
                "RabbitMq:Password"]
            ?? throw new InvalidOperationException(
                "RabbitMq:Password was not configured.");

        var portValue =
            configuration[
                "RabbitMq:Port"];

        var port =
            ushort.TryParse(
                portValue,
                out var configuredPort)
                ? configuredPort
                : (ushort)5672;

        var rabbitOptions =
            new RabbitMqOptions(
                host,
                port,
                virtualHost,
                username,
                password);

        services.AddSingleton(
            rabbitOptions);

        services.AddMassTransit(
            configurator =>
            {
                configurator.UsingRabbitMq(
                    (_, bus) =>
                    {
                        bus.Host(
                            rabbitOptions.Host,
                            rabbitOptions.Port,
                            rabbitOptions.VirtualHost,
                            hostConfigurator =>
                            {
                                hostConfigurator.Username(
                                    rabbitOptions.Username);

                                hostConfigurator.Password(
                                    rabbitOptions.Password);
                            });
                    });
            });

        services.AddScoped<
            ILoadProcessingPublisher,
            MassTransitLoadProcessingPublisher>();
    }
}