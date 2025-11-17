using Application;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Spectre.Console;

namespace CasusFda;

public class ConsoleBackgroundService : BackgroundService
{
    private readonly ICommandHandler<RunApplicationCommand, RunApplicationResult> _commandHandler;
    private readonly ILogger<ConsoleBackgroundService> _logger;
    private IOptions<AppOptions> _appOptions;

    public ConsoleBackgroundService(ICommandHandler<RunApplicationCommand, RunApplicationResult> commandHandler, IOptions<AppOptions> appOptions, ILogger<ConsoleBackgroundService> logger)
    {
        _appOptions = appOptions;
        _logger = logger;
        _commandHandler = commandHandler;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogTrace("Start ConsoleBackgroundService");
        
        _logger.LogInformation("Search queries: {Join}", string.Join(", ", _appOptions.Value.SearchQueries));
        var command = new RunApplicationCommand(_appOptions.Value.SearchQueries);
        
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