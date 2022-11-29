namespace WebScraper.Api.Data.Models
{
    public class UserAgentString
    {
        public int Id { get; set; }

        public string Agent { get; set; }

        public string? Product { get; set; }

        public string? Version { get; set; }
    }
}
