using Application;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string baseAddress)
    {
        services
            .AddOptions<FundaOptions>()
            .BindConfiguration(FundaOptions.Section)
            .ValidateOnStart();
        
        services.AddHttpClient<FundaClient>(clientOptions =>
        {
            clientOptions.BaseAddress = new Uri(baseAddress);
        });

        services.AddTransient<IRealEstateService, FundaDataService>();
        
        return services;
    }
}