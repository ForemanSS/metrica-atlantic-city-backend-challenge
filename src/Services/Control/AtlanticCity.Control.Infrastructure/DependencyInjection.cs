using AtlanticCity.Control.Application.Loads.PeriodReservation;
using AtlanticCity.Control.Infrastructure.Persistence;
using AtlanticCity.Control.Infrastructure.Persistence.PeriodReservation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace AtlanticCity.Control.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddControlInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<ControlDbContext>(
            options =>
                options.UseNpgsql(connectionString));

        services.AddSingleton(
            _ => NpgsqlDataSource.Create(connectionString));

        services.AddScoped<
            ILoadPeriodReservationStore,
            LoadPeriodReservationStore>();

        return services;
    }
}