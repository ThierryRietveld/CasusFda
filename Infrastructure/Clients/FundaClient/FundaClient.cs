using System.Net.Http.Json;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;

namespace Infrastructure.Clients.FundaClient;

public class FundaClient
{
    private readonly HttpClient _httpClient;
    private readonly ResiliencePipeline<HttpResponseMessage> _pipeline;
    private readonly IOptions<FundaOptions> _fundaOptions;
    private readonly ILogger<FundaClient> _logger;

    public FundaClient(HttpClient httpClient, ResiliencePipeline<HttpResponseMessage> pipeline, IOptions<FundaOptions> fundaOptions, ILogger<FundaClient> logger)
    {
        _httpClient = httpClient;
        _pipeline = pipeline;
        _fundaOptions = fundaOptions;
        _logger = logger;
    }

    public async Task<FundaApiResponse?> GetFundaObjectsAsync(
        string searchQuery,
        int page,
        int pageSize, 
        CancellationToken cancellationToken = default)
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

        var response = await _pipeline.ExecuteAsync(async _ =>
        {
            _logger.LogDebug("Page request: {Page}", page);
            return await _httpClient.GetAsync(urlWithQuery, cancellationToken);
        }, cancellationToken);

        response.EnsureSuccessStatusCode();
        
        return await response.Content
            .ReadFromJsonAsync<FundaApiResponse>(cancellationToken: cancellationToken);
    }
}