using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services
            .AddTransient<ICommandHandler<RunApplicationCommand, RunApplicationResult>, RunApplicationCommandHandler>();
        
        return services;
    }
}