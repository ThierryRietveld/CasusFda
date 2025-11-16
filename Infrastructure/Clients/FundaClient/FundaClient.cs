using System.Net.Http.Json;
using Infrastructure.Models;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace Infrastructure;

public class FundaClient
{
    private readonly HttpClient _httpClient;
    private readonly IOptions<FundaOptions> _fundaOptions;

    public FundaClient(HttpClient httpClient, IOptions<FundaOptions> fundaOptions)
    {
        _httpClient = httpClient;
        _fundaOptions = fundaOptions;
    }

    public async Task<FundaApiResponse?> GetFundaObjectsAsync(
        string searchQuery,
        int page,
        int pageSize)
    {
        var queryParams = new Dictionary<string, string?>
        {
            ["type"] = "koop",
            ["zo"] = searchQuery,
            ["page"] = page.ToString(),
            ["pagesize"] = pageSize.ToString()
        };

        var relativePath = $"{_fundaOptions.Value.ApiKey}/";

        var urlWithQuery = QueryHelpers.AddQueryString(relativePath, queryParams);

        var response = await _httpClient.GetAsync(urlWithQuery);

        response.EnsureSuccessStatusCode();
        
        return await response.Content
            .ReadFromJsonAsync<FundaApiResponse>();
    }
}