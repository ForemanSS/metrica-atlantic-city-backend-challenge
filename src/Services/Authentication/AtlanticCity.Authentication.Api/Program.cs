using AtlanticCity.Authentication.Infrastructure;
using AtlanticCity.Authentication.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder =
    WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var connectionString =
    builder.Configuration
        .GetConnectionString("AuthenticationDb")
    ?? throw new InvalidOperationException(
        "Connection string 'AuthenticationDb' was not configured.");

builder.Services.AddAuthenticationInfrastructure(
    connectionString);

var app =
    builder.Build();

await using (var scope =
             app.Services.CreateAsyncScope())
{
    var dbContext =
        scope.ServiceProvider
            .GetRequiredService<
                AuthenticationDbContext>();

    await dbContext.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();