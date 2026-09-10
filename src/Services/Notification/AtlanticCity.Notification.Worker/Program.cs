using AtlanticCity.Notification.Application;
using AtlanticCity.Notification.Infrastructure;

var builder =
    Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();

builder.Logging.AddJsonConsole(
    options =>
    {
        options.IncludeScopes = true;
        options.UseUtcTimestamp = true;
        options.TimestampFormat =
            "yyyy-MM-dd'T'HH:mm:ss.fff'Z'";
    });

builder.Services
    .AddNotificationApplication();

builder.Services
    .AddNotificationInfrastructure(
        builder.Configuration);

var host =
    builder.Build();

host.Run();