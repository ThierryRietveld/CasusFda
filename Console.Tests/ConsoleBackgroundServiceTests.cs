using Application;
using CasusFda;
using FakeItEasy;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Shouldly;
using Spectre.Console.Testing;

namespace Console.Tests;

public class ConsoleBackgroundServiceTests
{
    [Fact]
    public async Task ExecuteAsync_WithSimpleApplicationResult_ShouldRunWithoutErrors()
    {
        var commandHandler = A.Fake<ICommandHandler<RunApplicationCommand, RunApplicationResult>>();
        var appOptions = new AppOptions
        {
            SearchQueries = ["TestQuery"]
        };
        var logger = new NullLogger<ConsoleBackgroundService>();
        var console = new TestConsole();
        
        var result = new RunApplicationResult
        {
            Results = new List<SearchResult>
            {
                new()
                {
                    SearchQuery = "TestQuery",
                    MakelaarCount = new Dictionary<string, int>
                    {
                        { "Makelaar A", 2 },
                        { "Makelaar B", 1 }
                    }
                }
            }
        };

        A.CallTo(() => commandHandler.HandleAsync(A<RunApplicationCommand>._, A<CancellationToken>._))
            .Returns(result);
        
        var sut = new ConsoleBackgroundService(commandHandler, Options.Create(appOptions), console, logger);
        
        await sut.StartAsync(CancellationToken.None);
        
        console.Output.Trim().ShouldBe(""""
                                Search Query: TestQuery
                                ┌────────────┬─────────────────┐
                                │ Makelaar   │ Aantal objecten │
                                ├────────────┼─────────────────┤
                                │ Makelaar A │ 2               │
                                │ Makelaar B │ 1               │
                                └────────────┴─────────────────┘
                                """");
    }
    
    [Fact]
    public async Task ExecuteAsync_WithUnorderedApplicationResult_ShouldRunWithoutErrors()
    {
        var commandHandler = A.Fake<ICommandHandler<RunApplicationCommand, RunApplicationResult>>();
        var appOptions = new AppOptions
        {
            SearchQueries = ["TestQuery"]
        };
        var logger = new NullLogger<ConsoleBackgroundService>();
        var console = new TestConsole();
        
        var result = new RunApplicationResult
        {
            Results = new List<SearchResult>
            {
                new()
                {
                    SearchQuery = "TestQuery",
                    MakelaarCount = new Dictionary<string, int>
                    {
                        { "Makelaar A", 2 },
                        { "Makelaar B", 1 },
                        { "Makelaar C", 4 },
                        { "Makelaar D", 1 }
                    }
                }
            }
        };

        A.CallTo(() => commandHandler.HandleAsync(A<RunApplicationCommand>._, A<CancellationToken>._))
            .Returns(result);
        
        var sut = new ConsoleBackgroundService(commandHandler, Options.Create(appOptions), console, logger);
        
        await sut.StartAsync(CancellationToken.None);
        
        console.Output.Trim().ShouldBe(""""
                                       Search Query: TestQuery
                                       ┌────────────┬─────────────────┐
                                       │ Makelaar   │ Aantal objecten │
                                       ├────────────┼─────────────────┤
                                       │ Makelaar C │ 4               │
                                       │ Makelaar A │ 2               │
                                       │ Makelaar B │ 1               │
                                       │ Makelaar D │ 1               │
                                       └────────────┴─────────────────┘
                                       """");
    }
}