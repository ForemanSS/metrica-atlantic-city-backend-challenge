using AtlanticCity.Authentication.Application;
using AtlanticCity.Authentication.Infrastructure;
using AtlanticCity.Authentication.Infrastructure.Initialization;
using AtlanticCity.BuildingBlocks.Observability;

var builder =
    WebApplication.CreateBuilder(args);

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
    .AddAtlanticCityProblemDetails();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services
    .AddAuthenticationApplication();

builder.Services
    .AddAuthenticationInfrastructure(
        builder.Configuration);

var app =
    builder.Build();

app.UseAtlanticCityCorrelationId();

app.UseAtlanticCityExceptionHandling();

await app.Services
    .InitializeAuthenticationAsync();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();

app.Run();