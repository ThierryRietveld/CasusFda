namespace CasusFda;

public class AppOptions
{
    public static string Section = "App";
    public required IEnumerable<string> SearchQueries { get; set; }
}