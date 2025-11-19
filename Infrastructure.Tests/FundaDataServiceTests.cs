using Application.Models;
using FakeItEasy;
using Infrastructure.Clients.FundaClient;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Shouldly;
using Object = Infrastructure.Clients.FundaClient.Object;

namespace Infrastructure.Tests;

public class FundaDataServiceTests
{
    private FundaOptions _fundaOptions;

    public FundaDataServiceTests()
    {
        _fundaOptions = new FundaOptions
        {
            ApiKey = "api-key",
            BaseUrl = "https://api.funda.nl",
            PageSize = 25,
            RateLimit = new RateLimitOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.Zero
            },
            RetryOptions = new RetryOptions
            {
                MaxRetryAttempts = 5,
                Delay = TimeSpan.Zero,
                MaxDelay = TimeSpan.Zero
            }
        };
    }

    [Fact]
    public async Task GetFundaObjectsAsync_WithOptions_ShouldUseOptions()
    {
        // Arrange
        var client = A.Fake<IFundaClient>();
        var logger = new NullLogger<FundaDataService>();
        var sut = new FundaDataService(client, Options.Create(_fundaOptions), logger);
        var searchQuery = "Amsterdam";
        
        var capturedPageSize = A.Captured<int>();
        
        A.CallTo(() => client.GetFundaObjectsAsync(
            A<string>._,
            A<int>._,
            capturedPageSize._,
            A<CancellationToken>._)).Returns(new FundaApiResponse
        {
            Objects = []
        });
        
        // Act
        await sut.GetFundaObjectsAsync(searchQuery, CancellationToken.None);
        
        // Assert
        capturedPageSize.GetLastValue().ShouldBe(_fundaOptions.PageSize);
    }
    
    [Fact]
    public async Task GetFundaObjectsAsync_WithSearchQuery_ShouldUseSearchQuery()
    {
        // Arrange
        var client = A.Fake<IFundaClient>();
        var logger = new NullLogger<FundaDataService>();
        var sut = new FundaDataService(client, Options.Create(_fundaOptions), logger);
        var searchQuery = "Amsterdam";
        
        A.CallTo(() => client.GetFundaObjectsAsync(
            A<string>._,
            A<int>._,
            A<int>._,
            A<CancellationToken>._)).Returns(new FundaApiResponse
        {
            Objects = []
        });
        
        // Act
        await sut.GetFundaObjectsAsync(searchQuery, CancellationToken.None);
        
        // Assert
        A.CallTo(() => client.GetFundaObjectsAsync(
            searchQuery,
            A<int>._,
            A<int>._,
            A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }
    
    [Fact]
    public async Task GetFundaObjectsAsync_WithNullResponse_ShouldReturnEmptyList()
    {
        // Arrange
        var client = A.Fake<IFundaClient>();
        var logger = new NullLogger<FundaDataService>();
        var sut = new FundaDataService(client, Options.Create(_fundaOptions), logger);
        var searchQuery = "Amsterdam";
        
        A.CallTo(() => client.GetFundaObjectsAsync(
            A<string>._,
            A<int>._,
            A<int>._,
            A<CancellationToken>._)).Returns((FundaApiResponse)null!);
        
        // Act
        var result = await sut.GetFundaObjectsAsync(searchQuery, CancellationToken.None);
        
        // Assert
        result.ShouldBeEmpty();
    }
    
    [Fact]
    public async Task GetFundaObjectsAsync_WithEmptyResponse_ShouldReturnEmptyList()
    {
        // Arrange
        var client = A.Fake<IFundaClient>();
        var logger = new NullLogger<FundaDataService>();
        var sut = new FundaDataService(client, Options.Create(_fundaOptions), logger);
        var searchQuery = "Amsterdam";
        
        A.CallTo(() => client.GetFundaObjectsAsync(
            A<string>._,
            A<int>._,
            A<int>._,
            A<CancellationToken>._)).Returns(new FundaApiResponse
        {
            Objects = []
        });
        
        // Act
        var result = await sut.GetFundaObjectsAsync(searchQuery, CancellationToken.None);
        
        // Assert
        result.ShouldBeEmpty();
    }
    
    [Fact]
    public async Task GetFundaObjectsAsync_WithOneResponse_ShouldReturnCorrectFundaObjects()
    {
        // Arrange
        var client = A.Fake<IFundaClient>();
        var logger = new NullLogger<FundaDataService>();
        var sut = new FundaDataService(client, Options.Create(_fundaOptions), logger);
        var searchQuery = "Amsterdam";
        
        var makeLaars = new List<Object>
        {
            new()
            {
                MakelaarNaam = "Makelaar A"
            },
            new()
            {
                MakelaarNaam = "Makelaar B"
            }
        };
        
        A.CallTo(() => client.GetFundaObjectsAsync(
            A<string>._,
            A<int>._,
            A<int>._,
            A<CancellationToken>._)).ReturnsNextFromSequence(
                new FundaApiResponse
                {
                    Objects = makeLaars
                },
                new FundaApiResponse
                {
                    Objects = []
                });
        
        // Act
        var result = await sut.GetFundaObjectsAsync(searchQuery, CancellationToken.None);
        
        // Assert
        var expectedFundaObjects = new List<FundaObject>()
        {
            new()
            {
                MakelaarNaam = "Makelaar A"
            },
            new()
            {
                MakelaarNaam = "Makelaar B"
            }
        };
        
        result.Count().ShouldBe(2);
        result.ShouldBeEquivalentTo(expectedFundaObjects);
    }
    
    [Fact]
    public async Task GetFundaObjectsAsync_WithMultipoleResponses_ShouldReturnCorrectFundaObjects()
    {
        // Arrange
        var client = A.Fake<IFundaClient>();
        var logger = new NullLogger<FundaDataService>();
        var sut = new FundaDataService(client, Options.Create(_fundaOptions), logger);
        var searchQuery = "Amsterdam";
        
        var makeLaars1 = new List<Object>
        {
            new()
            {
                MakelaarNaam = "Makelaar A"
            },
            new()
            {
                MakelaarNaam = "Makelaar B"
            }
        };
        
        var makeLaars2 = new List<Object>
        {
            new()
            {
                MakelaarNaam = "Makelaar A"
            },
            new()
            {
                MakelaarNaam = "Makelaar C"
            }
        };
        
        A.CallTo(() => client.GetFundaObjectsAsync(
            A<string>._,
            A<int>._,
            A<int>._,
            A<CancellationToken>._)).ReturnsNextFromSequence(
            new FundaApiResponse
            {
                Objects = makeLaars1
            },
            new FundaApiResponse
            {
                Objects = makeLaars2
            },
            new FundaApiResponse
            {
                Objects = []
            });
        
        // Act
        var result = await sut.GetFundaObjectsAsync(searchQuery, CancellationToken.None);
        
        // Assert
        var expectedFundaObjects = new List<FundaObject>()
        {
            new()
            {
                MakelaarNaam = "Makelaar A"
            },
            new()
            {
                MakelaarNaam = "Makelaar B"
            },
            new()
            {
                MakelaarNaam = "Makelaar A"
            },
            new()
            {
                MakelaarNaam = "Makelaar C"
            }
        };
        
        result.Count().ShouldBe(4);
        result.ShouldBeEquivalentTo(expectedFundaObjects);
    }
    
    [Fact]
    public async Task GetFundaObjectsAsync_WithMultipoleResponses_ShouldIncrementPage()
    {
        // Arrange
        var client = A.Fake<IFundaClient>();
        var logger = new NullLogger<FundaDataService>();
        var sut = new FundaDataService(client, Options.Create(_fundaOptions), logger);
        var searchQuery = "Amsterdam";
        
        var makeLaars1 = new List<Object>
        {
            new()
            {
                MakelaarNaam = "Makelaar A"
            },
            new()
            {
                MakelaarNaam = "Makelaar B"
            }
        };
        
        A.CallTo(() => client.GetFundaObjectsAsync(
            A<string>._,
            1,
            A<int>._,
            A<CancellationToken>._)).Returns(
                new FundaApiResponse
                {
                    Objects = makeLaars1
                });
        
        A.CallTo(() => client.GetFundaObjectsAsync(
            A<string>._,
            2,
            A<int>._,
            A<CancellationToken>._)).Returns(
            new FundaApiResponse
            {
                Objects = []
            });
        
        // Act 
        await sut.GetFundaObjectsAsync(searchQuery, CancellationToken.None);
        
        // Assert
        A.CallTo(() => client.GetFundaObjectsAsync(
            A<string>._,
            1,
            A<int>._,
            A<CancellationToken>._)).MustHaveHappenedOnceExactly();
        
        A.CallTo(() => client.GetFundaObjectsAsync(
            A<string>._,
            2,
            A<int>._,
            A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }
}