namespace Infrastructure;

public class FundaOptions
{
    public static string Section => "FundaConfiguration";
    
    public required string ApiKey { get; init; }
    public required string BaseUrl { get; init; }
}