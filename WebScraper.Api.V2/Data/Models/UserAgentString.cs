namespace WebScraper.Api.V2.Data.Models;

public class UserAgentString
{
    public UserAgentString(string agent)
    {
        Agent = agent;
    }

    public int Id { get; set; }

    public string Agent { get; set; }

    public string? Product { get; set; }

    public string? Version { get; set; }
}