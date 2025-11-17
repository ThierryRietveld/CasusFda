using Application.Models;
using FakeItEasy;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;

namespace Application.Tests;

public class RunApplicationCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithOneQuery_ShouldReturnCorrectResults()
    {
        // Arrange
        var realEstateService = A.Fake<IRealEstateService>();
        var logger = new NullLogger<RunApplicationCommandHandler>();
        var command = new RunApplicationCommand(["testquery"]);
        
        var fundaObjects = new List<FundaObject>
        {
            new() { MakelaarNaam = "Makelaar A" },
            new() { MakelaarNaam = "Makelaar B" },
            new() { MakelaarNaam = "Makelaar A" }
        };

        A.CallTo(() => realEstateService.GetFundaObjectsAsync(A<string>._, A<CancellationToken>._)).Returns(fundaObjects);
        
        var sut = new RunApplicationCommandHandler(realEstateService, logger);
        
        // Act
        var result = await sut.HandleAsync(command);
        
        // Assert
        var expectedResults = new RunApplicationResult
        {
            Results = new List<SearchResult>
            {
                new()
                {
                    SearchQuery = "testquery",
                    MakelaarCount = new Dictionary<string, int>
                    {
                        { "Makelaar A", 2 },
                        { "Makelaar B", 1 }
                    }
                }
            }
        };
        
        result.ShouldBeEquivalentTo(expectedResults);
    }
    
    [Fact]
    public async Task Handle_WithTwoQueries_ShouldReturnCorrectResults()
    {
        // Arrange
        var realEstateService = A.Fake<IRealEstateService>();
        var logger = new NullLogger<RunApplicationCommandHandler>();
        var command = new RunApplicationCommand(["testquery1", "testquery2"]);
        
        var fundaObjects1 = new List<FundaObject>
        {
            new() { MakelaarNaam = "Makelaar A" },
            new() { MakelaarNaam = "Makelaar B" },
            new() { MakelaarNaam = "Makelaar A" }
        };
        
        var fundaObjects2 = new List<FundaObject>
        {
            new() { MakelaarNaam = "Makelaar A" },
            new() { MakelaarNaam = "Makelaar B" },
            new() { MakelaarNaam = "Makelaar A" },
            new() { MakelaarNaam = "Makelaar B" },
            new() { MakelaarNaam = "Makelaar B" }
        };

        A.CallTo(() => 
                realEstateService.GetFundaObjectsAsync(command.SearchQueries.ToList()[0], A<CancellationToken>._))
                .Returns(fundaObjects1);
        
        A.CallTo(() => 
                realEstateService.GetFundaObjectsAsync(command.SearchQueries.ToList()[1], A<CancellationToken>._))
            .Returns(fundaObjects2);
        
        var sut = new RunApplicationCommandHandler(realEstateService, logger);
        
        // Act
        var result = await sut.HandleAsync(command);
        
        // Assert
        var expectedResults = new RunApplicationResult
        {
            Results = new List<SearchResult>
            {
                new()
                {
                    SearchQuery = command.SearchQueries.ToList()[0],
                    MakelaarCount = new Dictionary<string, int>
                    {
                        { "Makelaar A", 2 },
                        { "Makelaar B", 1 }
                    }
                },
                new()
                {
                    SearchQuery = command.SearchQueries.ToList()[1],
                    MakelaarCount = new Dictionary<string, int>
                    {
                        { "Makelaar A", 2 },
                        { "Makelaar B", 3 }
                    }
                }
            }
        };
        
        result.ShouldBeEquivalentTo(expectedResults);
    }
    
    [Fact]
    public async Task Handle_WithNoQueries_ShouldReturnEmptyResult()
    {
        // Arrange
        var realEstateService = A.Fake<IRealEstateService>();
        var logger = new NullLogger<RunApplicationCommandHandler>();
        var command = new RunApplicationCommand([]);
        
        var sut = new RunApplicationCommandHandler(realEstateService, logger);
        
        // Act
        var result = await sut.HandleAsync(command);
         
        // Assert
        result.ShouldNotBeNull();
        result.Results.ShouldNotBeNull();
        result.Results.ShouldBeEmpty();
        
        A.CallTo(() => realEstateService.GetFundaObjectsAsync(A<string>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }
    
    [Fact]
    public async Task Handle_WithQueryReturningNoObjects_ShouldReturnEmptyResult()
    {
        // Arrange
        var realEstateService = A.Fake<IRealEstateService>();
        var logger = new NullLogger<RunApplicationCommandHandler>();
        var command = new RunApplicationCommand(["testquery"]);

        A.CallTo(() => realEstateService.GetFundaObjectsAsync("testquery", A<CancellationToken>._))
            .Returns(new List<FundaObject>());

        var sut = new RunApplicationCommandHandler(realEstateService, logger);

        // Act
        var result = await sut.HandleAsync(command);

        // Assert
        result.Results.Count().ShouldBe(1);
        var searchResult = result.Results.Single();
        searchResult.SearchQuery.ShouldBe("testquery");
        searchResult.MakelaarCount.ShouldNotBeNull();
        searchResult.MakelaarCount.ShouldBeEmpty();
    }
}