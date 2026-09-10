using AtlanticCity.MassiveLoad.Application;
using AtlanticCity.MassiveLoad.Infrastructure;

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
    .AddMassiveLoadApplication();

builder.Services
    .AddMassiveLoadInfrastructure(
        builder.Configuration);

var host =
    builder.Build();

host.Run();