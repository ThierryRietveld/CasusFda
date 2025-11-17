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
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, FundaOptions fundaOptions)
    {
        services
            .AddOptions<FundaOptions>()
            .BindConfiguration(FundaOptions.Section)
            .ValidateOnStart();
        
        services.AddSingleton<ResiliencePipeline<HttpResponseMessage>>(sp =>
        {
            var rateLimiter = new FixedWindowRateLimiter(new FixedWindowRateLimiterOptions()
            {
                PermitLimit = fundaOptions.RateLimit.PermitLimit,
                Window = fundaOptions.RateLimit.Window,
                QueueLimit = int.MaxValue,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst
            });
            
            var retryOptions = new RetryStrategyOptions<HttpResponseMessage>
            {
                MaxRetryAttempts = fundaOptions.RetryOptions.MaxRetryAttempts,
                Delay = fundaOptions.RetryOptions.Delay,
                MaxDelay = fundaOptions.RetryOptions.MaxDelay,
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = false,
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
            clientOptions.BaseAddress = new Uri(fundaOptions.BaseUrl);
        });

        services.AddTransient<IRealEstateService, FundaDataService>();
        
        return services;
    }
}