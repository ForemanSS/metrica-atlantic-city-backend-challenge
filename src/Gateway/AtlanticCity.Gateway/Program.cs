using AtlanticCity.BuildingBlocks.Observability;
using AtlanticCity.Gateway.Endpoints;
using AtlanticCity.Gateway.Extensions;

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

builder.Services.AddGatewayServices(
    builder.Configuration);

var app =
    builder.Build();

app.UseAtlanticCityCorrelationId();

app.UseAtlanticCityExceptionHandling();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.UseRateLimiter();

app.MapGet(
        "/health",
        () =>
            Results.Ok(
                new
                {
                    service =
                        "AtlanticCity.Gateway",
                    status =
                        "Healthy"
                }))
    .AllowAnonymous();

app.MapGatewayIdentityEndpoints();

app.MapReverseProxy();

app.Run();