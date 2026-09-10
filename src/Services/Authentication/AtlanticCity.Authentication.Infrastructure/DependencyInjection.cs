using System.Text;
using AtlanticCity.Authentication.Application.Security;
using AtlanticCity.Authentication.Application.Tokens;
using AtlanticCity.Authentication.Application.Users;
using AtlanticCity.Authentication.Infrastructure.Initialization;
using AtlanticCity.Authentication.Infrastructure.Persistence;
using AtlanticCity.Authentication.Infrastructure.Persistence.Repositories;
using AtlanticCity.Authentication.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AtlanticCity.Authentication.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAuthenticationInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString(
                "AuthenticationDb")
            ?? throw new InvalidOperationException(
                "Connection string 'AuthenticationDb' was not configured.");

        services.AddDbContext<AuthenticationDbContext>(
            options =>
                options.UseNpgsql(
                    connectionString));

        services.AddScoped<
            IUserRepository,
            UserRepository>();

        services.AddSingleton<
            IPasswordHashService,
            PasswordHashService>();

        services.AddSingleton<
            IAccessTokenService,
            JwtAccessTokenService>();

        services.AddScoped<
            AuthenticationDatabaseInitializer>();

        services.AddScoped<
            IRefreshTokenRepository,
            RefreshTokenRepository>();

        services.AddSingleton<
            IRefreshTokenService,
            RefreshTokenService>();

        ConfigureJwt(
            services,
            configuration);

        ConfigureRefreshToken(
            services,
            configuration);

        ConfigureSeedUser(
            services,
            configuration);

        return services;
    }

    private static void ConfigureJwt(
        IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<JwtOptions>()
            .Bind(
                configuration.GetSection(
                    JwtOptions.SectionName))
            .Validate(
                options =>
                    !string.IsNullOrWhiteSpace(
                        options.Issuer),
                "Jwt:Issuer is required.")
            .Validate(
                options =>
                    !string.IsNullOrWhiteSpace(
                        options.Audience),
                "Jwt:Audience is required.")
            .Validate(
                options =>
                    !string.IsNullOrWhiteSpace(
                        options.SigningKey) &&
                    Encoding.UTF8.GetByteCount(
                        options.SigningKey) >= 32,
                "Jwt:SigningKey must contain at least 32 bytes.")
            .Validate(
                options =>
                    options.AccessTokenMinutes
                    is >= 1 and <= 60,
                "Jwt:AccessTokenMinutes must be between 1 and 60.")
            .ValidateOnStart();
    }

    private static void ConfigureRefreshToken(
        IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<RefreshTokenOptions>()
            .Bind(
                configuration.GetSection(
                    RefreshTokenOptions.SectionName))
            .Validate(
                options =>
                    options.LifetimeDays
                        is >= 1 and <= 30,
                "RefreshToken:LifetimeDays must be between 1 and 30.")
            .ValidateOnStart();
    }

    private static void ConfigureSeedUser(
        IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<SeedUserOptions>()
            .Bind(
                configuration.GetSection(
                    SeedUserOptions.SectionName));
    }
}