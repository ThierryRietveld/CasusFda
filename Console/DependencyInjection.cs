using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
        
        services.AddLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddSimpleConsole(options =>
            {
                options.IncludeScopes = true;
                options.SingleLine = false;
                options.TimestampFormat = "HH:mm:ss zzz ";
            });
        });

        return services;
    }
}