namespace Infrastructure.Models;

public class FundaApiResponse
{
    public required IEnumerable<Object> Objects { get; set; }
}

public class Object
{
    public required string MakelaarNaam { get; set; }
}