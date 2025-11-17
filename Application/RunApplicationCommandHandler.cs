using Application.Models;
using Microsoft.Extensions.Logging;

namespace Application;

public class RunApplicationCommandHandler : ICommandHandler<RunApplicationCommand, RunApplicationResult>
{
    private readonly IRealEstateService _realEstateService;
    private readonly ILogger<RunApplicationCommandHandler> _logger;

    public RunApplicationCommandHandler(IRealEstateService realEstateService, ILogger<RunApplicationCommandHandler> logger)
    {
        _logger = logger;
        _realEstateService = realEstateService;
    }
    
    public async Task<RunApplicationResult> HandleAsync(RunApplicationCommand command, CancellationToken cancellationToken = default)
    {
        if (!command.SearchQueries.Any())
        {
            _logger.LogWarning("No search queries provided");
        }
        
        var resultPerSearchQuery = new Dictionary<string, IEnumerable<FundaObject>>();
        
        foreach (var query in command.SearchQueries)
        {
            using var scope = _logger.BeginScope(new Dictionary<string, object?>
            {
                ["SearchQuery"] = query
            });
            
            var results = await _realEstateService.GetFundaObjectsAsync(query, cancellationToken);
            resultPerSearchQuery.Add(query, results);
        }

        return new RunApplicationResult
        {
            Results = resultPerSearchQuery.Select(x =>
            {
                return new SearchResult
                {
                    SearchQuery = x.Key,
                    MakelaarCount = x.Value
                        .GroupBy(m => m.MakelaarNaam)
                        .ToDictionary(g => g.Key, g => g.Count())
                };
            }).ToList()
        };
    }
}