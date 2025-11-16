namespace Infrastructure.Clients.FundaClient;

public class FundaApiResponse
{
    public required IEnumerable<Object> Objects { get; set; }
}

public class Object
{
    public required string MakelaarNaam { get; set; }
}