using Application;
using Application.Models;
using Infrastructure.Clients.FundaClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure;

public class FundaDataService : IRealEstateService
{
    private readonly FundaClient _fundaClient;
    private readonly IOptions<FundaOptions> _fundaOptions;
    private readonly ILogger<FundaDataService> _logger;

    public FundaDataService(FundaClient fundaClient, IOptions<FundaOptions> fundaOptions, ILogger<FundaDataService> logger)
    {
        _logger = logger;
        _fundaOptions = fundaOptions;
        _fundaClient = fundaClient;
    }
    
    public async Task<IEnumerable<FundaObject>> GetFundaObjectsAsync(string searchQuery, CancellationToken cancellationToken = default)
    {
        var results = new List<FundaObject>();
        var page = 1;
        
        while (true)
        {
            var response = await _fundaClient.GetFundaObjectsAsync(searchQuery, page, _fundaOptions.Value.PageSize, cancellationToken);

            if (response is null)
            {
                _logger.LogError("Funda response is null for search query: {SearchQuery} on page {Page}", searchQuery, page);
                break;
            }
            
            results.AddRange(response.Objects.Select(x => new FundaObject
            {
                MakelaarNaam = x.MakelaarNaam
            }));

            page++;
            
            if (!response.Objects.Any()) break;
        }

        return results;
    }
}