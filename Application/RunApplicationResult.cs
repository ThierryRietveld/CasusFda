namespace Application;

public class RunApplicationResult
{
    public required IEnumerable<SearchResult> results { get; set; }
}

public class SearchResult
{
    public required string SearchQuery { get; set; }
    public required Dictionary<string, int> MakelaarCount { get; set; }
}