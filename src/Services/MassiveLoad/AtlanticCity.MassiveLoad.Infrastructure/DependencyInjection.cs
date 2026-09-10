using AtlanticCity.MassiveLoad.Application.Storage;
using AtlanticCity.MassiveLoad.Infrastructure.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AtlanticCity.MassiveLoad.Application.Excel;
using AtlanticCity.MassiveLoad.Infrastructure.Excel;
using AtlanticCity.MassiveLoad.Application.Persistence;
using AtlanticCity.MassiveLoad.Infrastructure.Persistence;
using Npgsql;
using AtlanticCity.BuildingBlocks.Messaging;
using AtlanticCity.MassiveLoad.Application.Messaging;
using AtlanticCity.MassiveLoad.Infrastructure.Messaging;
using MassTransit;

namespace AtlanticCity.MassiveLoad.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddMassiveLoadInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(
            configuration);

        var connectionString =
            configuration.GetConnectionString(
                "MassiveLoadDb")
            ?? throw new InvalidOperationException(
                "ConnectionStrings:MassiveLoadDb was not configured.");

        services.AddSingleton(
            _ =>
                NpgsqlDataSource.Create(
                    connectionString));

        services.AddScoped<
            IMassiveLoadStore,
            MassiveLoadStore>();

        ConfigureSeaweedFs(
            services,
            configuration);

        ConfigureMessaging(
            services,
            configuration);

        services.AddScoped<
            IExcelLoadReader,
            ExcelDataReaderLoadReader>();

        return services;
    }

    private static void ConfigureSeaweedFs(
        IServiceCollection services,
        IConfiguration configuration)
    {
        var baseAddress =
            configuration["SeaweedFs:BaseAddress"]
            ?? "http://localhost:8888/";

        if (!Uri.TryCreate(
                baseAddress,
                UriKind.Absolute,
                out var baseUri))
        {
            throw new InvalidOperationException(
                "SeaweedFs:BaseAddress is invalid.");
        }

        services.AddSingleton(
            new SeaweedLoadFileSourceOptions(
                baseUri.ToString()));

        services.AddHttpClient<
            ILoadFileSource,
            SeaweedLoadFileSource>(
        client =>
        {
            client.BaseAddress =
                baseUri;

            client.Timeout =
                TimeSpan.FromMinutes(2);
        })
        .AddStandardResilienceHandler(
        options =>
        {
            options.Retry.MaxRetryAttempts =
                1;

            options.Retry.Delay =
                TimeSpan.FromMilliseconds(500);

            options.Retry.UseJitter =
                true;

            options.AttemptTimeout.Timeout =
                TimeSpan.FromSeconds(20);

            options.TotalRequestTimeout.Timeout =
                TimeSpan.FromSeconds(45);

            options.CircuitBreaker.FailureRatio =
                0.5;

            options.CircuitBreaker.MinimumThroughput =
                4;

            options.CircuitBreaker.SamplingDuration =
                TimeSpan.FromSeconds(60);

            options.CircuitBreaker.BreakDuration =
                TimeSpan.FromSeconds(15);
        });

        return;
    }

    private static void ConfigureMessaging(
        IServiceCollection services,
        IConfiguration configuration)
    {
        var host =
            configuration["RabbitMq:Host"]
            ?? "localhost";

        var virtualHost =
            configuration["RabbitMq:VirtualHost"]
            ?? "/";

        var username =
            configuration["RabbitMq:Username"]
            ?? throw new InvalidOperationException(
                "RabbitMq:Username was not configured.");

        var password =
            configuration["RabbitMq:Password"]
            ?? throw new InvalidOperationException(
                "RabbitMq:Password was not configured.");

        var portValue =
            configuration["RabbitMq:Port"];

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
                configurator.AddConsumer<
                    ProcessMassiveLoadConsumer>();

                configurator.UsingRabbitMq(
                    (context, bus) =>
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

                        bus.ReceiveEndpoint(
                            QueueNames.MassiveLoad,
                            endpoint =>
                            {
                                endpoint.ExchangeType =
                                    "direct";

                                endpoint.Durable =
                                    true;

                                endpoint.AutoDelete =
                                    false;

                                endpoint.UseMessageRetry(
                                    retry =>
                                        retry.Intervals(
                                            TimeSpan.FromSeconds(2),
                                            TimeSpan.FromSeconds(5),
                                            TimeSpan.FromSeconds(10)));

                                endpoint.ConfigureConsumer<
                                    ProcessMassiveLoadConsumer>(
                                    context);
                            });
                    });
            });

        services.AddScoped<
            ILoadCompletionPublisher,
            MassTransitLoadCompletionPublisher>();
    }
}