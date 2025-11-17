namespace CasusFda;

public class AppOptions
{
    public const string Section = "App";
    public required IEnumerable<string> SearchQueries { get; set; }
}