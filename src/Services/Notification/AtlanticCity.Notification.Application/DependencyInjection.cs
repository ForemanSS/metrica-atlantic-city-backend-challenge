using AtlanticCity.Notification.Application.Notifications;
using Microsoft.Extensions.DependencyInjection;

namespace AtlanticCity.Notification.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddNotificationApplication(
        this IServiceCollection services)
    {
        services.AddScoped<
            SendLoadNotificationCommandHandler>();

        return services;
    }
}