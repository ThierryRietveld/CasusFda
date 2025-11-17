using Application;
using Application.Models;
using Infrastructure.Clients.FundaClient;

namespace Infrastructure;

public class FundaDataService : IRealEstateService
{
    private readonly FundaClient _fundaClient;

    public FundaDataService(FundaClient fundaClient)
    {
        _fundaClient = fundaClient;
    }
    
    public async Task<IEnumerable<FundaObject>> GetFundaObjectsAsync(string searchQuery, CancellationToken cancellationToken = default)
    {
        var results = new List<FundaObject>();
        var page = 1;
        
        while (true)
        {
            var response = await _fundaClient.GetFundaObjectsAsync(searchQuery, page, 25, cancellationToken);
            
            if (response is null) break;
            
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