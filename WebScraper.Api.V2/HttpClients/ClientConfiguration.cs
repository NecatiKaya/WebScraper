using WebScraper.Api.V2.Logging;

namespace WebScraper.Api.V2.HttpClients;
public class ClientConfiguration
{
    public ILogger? Logger{ get; set; }

    public ApplicationLogModelJar? LoggingJar { get; set; }
}
