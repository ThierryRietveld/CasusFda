namespace Infrastructure;

public class FundaOptions
{
    public const string Section = "FundaOptions";
    
    public required string ApiKey { get; init; }
    public required string BaseUrl { get; init; }
    public required int PageSize { get; init; }
    public required RateLimitOptions RateLimit { get; init; }
    public required RetryOptions RetryOptions { get; init; }
}

public class RateLimitOptions
{
    public required int PermitLimit { get; init; }
    public required TimeSpan Window { get; init; }
}

public class RetryOptions
{
    public required int MaxRetryAttempts { get; init; }
    public required TimeSpan Delay { get; init; }
    public required TimeSpan MaxDelay { get; init; }
}