using AtlanticCity.Authentication.Application.Login;
using AtlanticCity.Authentication.Application.RefreshTokens;
using AtlanticCity.Authentication.Application.RevokeToken;
using Microsoft.Extensions.DependencyInjection;

namespace AtlanticCity.Authentication.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddAuthenticationApplication(
        this IServiceCollection services)
    {
        services.AddScoped<LoginCommandHandler>();

        services.AddScoped<
            RefreshTokenCommandHandler>();

        services.AddScoped<
            RevokeTokenCommandHandler>();

        return services;
    }
}