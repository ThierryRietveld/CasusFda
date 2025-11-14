using Microsoft.Extensions.Hosting;

namespace CasusFda;

public class ConsoleBackgroundService : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        throw new NotImplementedException();
    }
}