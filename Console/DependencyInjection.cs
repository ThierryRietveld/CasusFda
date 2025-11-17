using Application;
using Microsoft.Extensions.DependencyInjection;

namespace CasusFda;

public static class DependencyInjection
{
    public static IServiceCollection AddConsole(this IServiceCollection services)
    {
        services
            .AddOptions<AppOptions>()
            .BindConfiguration(AppOptions.Section)
            .ValidateOnStart();
        
        services.AddHostedService<ConsoleBackgroundService>();

        return services;
    }
}