using System.Net;
using System.Threading.RateLimiting;
using Application;
using Infrastructure.Clients.FundaClient;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Retry;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string baseAddress)
    {
        services
            .AddOptions<FundaOptions>()
            .BindConfiguration(FundaOptions.Section)
            .ValidateOnStart();
        
        services.AddSingleton<ResiliencePipeline<HttpResponseMessage>>(sp =>
        {
            var rateLimiter = new FixedWindowRateLimiter(new FixedWindowRateLimiterOptions()
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = int.MaxValue,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            });
            
            var retryOptions = new RetryStrategyOptions<HttpResponseMessage>
            {
                MaxRetryAttempts = 5,
                Delay = TimeSpan.FromSeconds(5),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    .Handle<HttpRequestException>()
                    .HandleResult(r =>
                        (int)r.StatusCode >= 500 ||
                        r.StatusCode == HttpStatusCode.BadRequest ||
                        r.StatusCode == HttpStatusCode.Unauthorized)
            };

            return new ResiliencePipelineBuilder<HttpResponseMessage>()
                .AddRetry(retryOptions)
                .AddRateLimiter(rateLimiter)
                .Build();
        });
        
        services.AddHttpClient<FundaClient>(clientOptions =>
        {
            clientOptions.BaseAddress = new Uri(baseAddress);
        });

        services.AddTransient<IRealEstateService, FundaDataService>();
        
        return services;
    }
}