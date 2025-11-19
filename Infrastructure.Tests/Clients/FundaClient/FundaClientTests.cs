using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using Infrastructure.Clients.FundaClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Polly;
using Shouldly;
using Object = Infrastructure.Clients.FundaClient.Object;

namespace Infrastructure.Tests.Clients.FundaClient;

public class FundaClientTests
{
    private readonly ResiliencePipeline<HttpResponseMessage> _pipeline;
    private readonly FundaOptions _fundaOptions;

    public FundaClientTests()
    {
        _fundaOptions = new FundaOptions
        {
            ApiKey = "api-key",
            BaseUrl = "https://api.funda.nl",
            PageSize = 25,
            RateLimit = new RateLimitOptions
            {
                PermitLimit = 2,
                Window = TimeSpan.FromMilliseconds(5000)
            },
            RetryOptions = new RetryOptions
            {
                MaxRetryAttempts = 2,
                Delay = TimeSpan.FromSeconds(2),
                MaxDelay = TimeSpan.FromSeconds(10)
            }
        };
        
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddInfrastructure(_fundaOptions);
        var serviceProvider = serviceCollection.BuildServiceProvider();
        
        _pipeline = serviceProvider.GetRequiredService<ResiliencePipeline<HttpResponseMessage>>();
    }
    
    [Fact]
    public async Task GetFundaObjectsAsync_WithOneResponse_ShouldReturnResponse()
    {
        // Arrange
        var logger = new NullLogger<Infrastructure.Clients.FundaClient.FundaClient>();

        var httpClient = CreateHttpClient(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new FundaApiResponse { Objects = 
                    [
                        new Object
                        {
                            MakelaarNaam = "Test Makelaar"
                        }
                    ] 
                })
            }
        );
        
        var sut = new Infrastructure.Clients.FundaClient.FundaClient(httpClient, _pipeline, Options.Create(_fundaOptions), logger);
        
        // Act
        var result = await sut.GetFundaObjectsAsync("Amsterdam", 1, 25);
        
        // Assert
        var expectedFundaApiResponse = new FundaApiResponse
        {
            Objects = new List<Object>()
            {
                new()
                {
                    MakelaarNaam = "Test Makelaar"
                }
            }
        };
        
        result.ShouldBeEquivalentTo(expectedFundaApiResponse);
    }
    
    [Fact]
    public async Task GetFundaObjectsAsync_WithFailingResponse_ShouldRetryResponse()
    {
        // Arrange
        var logger = new NullLogger<Infrastructure.Clients.FundaClient.FundaClient>();

        var httpClient = CreateHttpClient(
            new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = null
            },
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new FundaApiResponse { Objects = 
                    [
                        new Object
                        {
                            MakelaarNaam = "Test Makelaar"
                        }
                    ]
                })
            }
        );
        
        var sut = new Infrastructure.Clients.FundaClient.FundaClient(httpClient, _pipeline, Options.Create(_fundaOptions), logger);
        
        // Act
        var result = await sut.GetFundaObjectsAsync("Amsterdam", 1, 25);
        
        // Assert
        var expectedFundaApiResponse = new FundaApiResponse
        {
            Objects = new List<Object>
            {
                new()
                {
                    MakelaarNaam = "Test Makelaar"
                }
            }
        };
        
        result.ShouldBeEquivalentTo(expectedFundaApiResponse);
    }
    
    [Fact]
    public async Task GetFundaObjectsAsync_WithTooManyRequestsInWindow_ShouldWaitForNextWindow()
    {
        // Arrange
        var logger = new NullLogger<Infrastructure.Clients.FundaClient.FundaClient>();

        var httpClient = CreateHttpClient(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new FundaApiResponse { Objects = 
                    [
                        new Object
                        {
                            MakelaarNaam = "Test Makelaar 1"
                        }
                    ]
                })
            },
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new FundaApiResponse { Objects = 
                    [
                        new Object
                        {
                            MakelaarNaam = "Test Makelaar 2"
                        }
                    ]
                })
            },
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new FundaApiResponse { Objects = 
                    [
                        new Object
                        {
                            MakelaarNaam = "Test Makelaar 3"
                        }
                    ]
                })
            }
        );
        
        var sut = new Infrastructure.Clients.FundaClient.FundaClient(httpClient, _pipeline, Options.Create(_fundaOptions), logger);
        
        var sw = Stopwatch.StartNew();
        
        // Act
        await sut.GetFundaObjectsAsync("Amsterdam", 1, 25);
        await sut.GetFundaObjectsAsync("Amsterdam", 1, 25);
        await sut.GetFundaObjectsAsync("Amsterdam", 1, 25);
        
        sw.Stop();
        
        // Assert
        sw.Elapsed.ShouldBeGreaterThanOrEqualTo(TimeSpan.FromMilliseconds(5000).Add(TimeSpan.FromMilliseconds(-200)));
    }
    
    private HttpClient CreateHttpClient(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handlerFunc)
    {
        var handler = new TestHttpMessageHandler(handlerFunc);
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri(_fundaOptions.BaseUrl)
        };
        return httpClient;
    }
    
    private HttpClient CreateHttpClient(params HttpResponseMessage[] responses)
    {
        var queue = new Queue<HttpResponseMessage>(responses);

        return CreateHttpClient((_, _) =>
        {
            if (queue.Count == 0)
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
            }

            return Task.FromResult(queue.Dequeue());
        });
    }
    
    private class TestHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _handler;

        public HttpRequestMessage? LastRequest { get; private set; }

        public TestHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler)
        {
            _handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            LastRequest = request;
            return _handler(request, cancellationToken);
        }
    }
}