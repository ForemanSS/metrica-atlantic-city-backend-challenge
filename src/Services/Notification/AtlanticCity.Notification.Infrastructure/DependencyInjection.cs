using AtlanticCity.BuildingBlocks.Messaging;
using AtlanticCity.Notification.Application.Email;
using AtlanticCity.Notification.Application.Persistence;
using AtlanticCity.Notification.Infrastructure.Email;
using AtlanticCity.Notification.Infrastructure.Messaging;
using AtlanticCity.Notification.Infrastructure.Persistence;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace AtlanticCity.Notification.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddNotificationInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString(
                "NotificationDb")
            ?? throw new InvalidOperationException(
                "ConnectionStrings:NotificationDb was not configured.");

        services.AddSingleton(
            _ =>
                NpgsqlDataSource.Create(
                    connectionString));

        services.AddScoped<
            INotificationStore,
            NotificationStore>();

        ConfigureSmtp(
            services,
            configuration);

        ConfigureMessaging(
            services,
            configuration);

        return services;
    }

    private static void ConfigureSmtp(
        IServiceCollection services,
        IConfiguration configuration)
    {
        var options =
            new SmtpOptions(
                configuration["Smtp:Host"]
                    ?? "localhost",
                configuration.GetValue<int?>(
                    "Smtp:Port")
                    ?? 1025,
                configuration["Smtp:FromAddress"]
                    ?? "no-reply@atlanticcity.local",
                configuration["Smtp:FromName"]
                    ?? "Atlantic City",
                configuration["Smtp:Username"],
                configuration["Smtp:Password"]);

        services.AddSingleton(
            options);

        services.AddScoped<
            INotificationEmailSender,
            MailKitNotificationEmailSender>();
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

        var port =
            configuration.GetValue<ushort?>(
                "RabbitMq:Port")
            ?? 5672;

        services.AddMassTransit(
            configurator =>
            {
                configurator.AddConsumer<
                    MassiveLoadCompletedConsumer>();

                configurator.UsingRabbitMq(
                    (context, bus) =>
                    {
                        bus.Host(
                            host,
                            port,
                            virtualHost,
                            h =>
                            {
                                h.Username(
                                    username);

                                h.Password(
                                    password);
                            });

                        bus.ReceiveEndpoint(
                            QueueNames.Notifications,
                            endpoint =>
                            {
                                endpoint.ExchangeType =
                                    "direct";

                                endpoint.Durable =
                                    true;

                                endpoint.AutoDelete =
                                    false;

                                endpoint.ConcurrentMessageLimit =
                                    1;

                                endpoint.UseMessageRetry(
                                    retry =>
                                        retry.Intervals(
                                            TimeSpan.FromSeconds(2),
                                            TimeSpan.FromSeconds(5),
                                            TimeSpan.FromSeconds(10)));

                                endpoint.ConfigureConsumer<
                                    MassiveLoadCompletedConsumer>(
                                    context);
                            });
                    });
            });
    }
}