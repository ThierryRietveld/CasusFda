using Application;
using Microsoft.Extensions.Hosting;

namespace CasusFda;

public class ConsoleBackgroundService : BackgroundService
{
    private readonly ICommandHandler<RunApplicationCommand, RunApplicationResult> _commandHandler;

    public ConsoleBackgroundService(ICommandHandler<RunApplicationCommand, RunApplicationResult> commandHandler)
    {
        _commandHandler = commandHandler;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var command = new RunApplicationCommand(["/amsterdam/"]);
        
        var result = await _commandHandler.HandleAsync(command, stoppingToken);
        
        foreach (var searchResult in result.Results)
        {
            Console.WriteLine($"Search Query: {searchResult.SearchQuery}");
            Console.WriteLine("Makelaar Counts:");
            foreach (var makelaarCount in searchResult.MakelaarCount.OrderBy(x => -x.Value).Take(10))
            {
                Console.WriteLine($"- {makelaarCount.Key}: {makelaarCount.Value}");
            }
            Console.WriteLine();
        }
    }
}