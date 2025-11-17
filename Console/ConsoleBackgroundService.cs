using Application;
using Microsoft.Extensions.Hosting;
using Spectre.Console;

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
        var command = new RunApplicationCommand(["/woerden/tuin/", "/harmelen/tuin/"]);
        
        var result = await _commandHandler.HandleAsync(command, stoppingToken);
        
        foreach (var searchResult in result.Results)
        {
            AnsiConsole.MarkupLine($"Search Query: [bold]{searchResult.SearchQuery}[/]");
            
            var table = new Table();

            table.AddColumn("[bold green]Makelaar[/]");
            table.AddColumn("[bold yellow]Aantal objecten[/]");
            
            foreach (var makelaarCount in searchResult.MakelaarCount.OrderBy(x => -x.Value).Take(10))
            {
                table.AddRow($"[bold]{makelaarCount.Key}[/]", makelaarCount.Value.ToString());
            }
            AnsiConsole.Write(table);
        }
    }
}