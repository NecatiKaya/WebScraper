namespace WebScraper.Api.V2.Data.Dto.UserAgentString;

public class UserAgentStringDto
{
    public UserAgentStringDto(string agent)
    {
        Agent = agent;
    }

    public int Id { get; set; }

    public string Agent { get; set; }

    public string? Product { get; set; }

    public string? Version { get; set; }
}