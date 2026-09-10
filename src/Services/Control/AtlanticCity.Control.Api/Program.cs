using AtlanticCity.Control.Api.Security;
using AtlanticCity.Control.Application;
using AtlanticCity.Control.Application.Loads.Upload;
using AtlanticCity.Control.Infrastructure;
using AtlanticCity.Control.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
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

var maxFileSizeBytes =
    builder.Configuration.GetValue<long?>(
        "LoadUpload:MaxFileSizeBytes")
    ?? LoadUploadSettings
        .DefaultMaxFileSizeBytes;

if (maxFileSizeBytes <= 0)
{
    throw new InvalidOperationException(
        "LoadUpload:MaxFileSizeBytes must be greater than zero.");
}

builder.Services.Configure<FormOptions>(
    options =>
    {
        options.MultipartBodyLengthLimit =
            maxFileSizeBytes;
    });

builder.Services.AddControlApplication(
    new LoadUploadSettings(
        maxFileSizeBytes));

builder.Services.AddControlInfrastructure(
    builder.Configuration);

builder.Services.AddControlSecurity(
    builder.Configuration);

var app =
    builder.Build();

app.UseAtlanticCityCorrelationId();

app.UseAtlanticCityExceptionHandling();

await using (var scope =
             app.Services.CreateAsyncScope())
{
    var dbContext =
        scope.ServiceProvider
            .GetRequiredService<ControlDbContext>();

    await dbContext.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();