using AtlanticCity.Control.Infrastructure;
using AtlanticCity.Control.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var connectionString =
    builder.Configuration.GetConnectionString("ControlDb")
    ?? throw new InvalidOperationException(
        "Connection string 'ControlDb' was not configured.");

builder.Services.AddControlInfrastructure(connectionString);

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext =
        scope.ServiceProvider.GetRequiredService<ControlDbContext>();

    await dbContext.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();