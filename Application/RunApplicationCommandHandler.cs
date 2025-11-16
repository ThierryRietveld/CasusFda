using Application.Models;

namespace Application;

public class RunApplicationCommandHandler : ICommandHandler<RunApplicationCommand, RunApplicationResult>
{
    private readonly IRealEstateService _realEstateService;

    public RunApplicationCommandHandler(IRealEstateService realEstateService)
    {
        _realEstateService = realEstateService;
    }
    
    public async Task<RunApplicationResult> HandleAsync(RunApplicationCommand command, CancellationToken cancellationToken = default)
    {
        var resultPerSearchQuery = new Dictionary<string, IEnumerable<FundaObject>>();
        
        foreach (var query in command.SearchQueries)
        {
            var results = await _realEstateService.GetFundaObjectsAsync(query);
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
            })
        };
    }
}