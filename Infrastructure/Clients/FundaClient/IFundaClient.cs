namespace Infrastructure.Clients.FundaClient;

public interface IFundaClient
{
    Task<FundaApiResponse?> GetFundaObjectsAsync(
        string searchQuery,
        int page,
        int pageSize, 
        CancellationToken cancellationToken = default);
}