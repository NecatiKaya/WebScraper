using Flurl.Http;
using Flurl.Http.Configuration;

namespace WebScraper.Api.V2.HttpClients.Flurl;

public class AmazonFlurlHttpClient : CrawlerHttpClientBase
{
    private readonly IFlurlClient _flurlClient;

    public AmazonFlurlHttpClient(IFlurlClientFactory flurlClientFac)
    {
        _flurlClient = flurlClientFac.Get("https://www.amazon.com.tr/");
    }

    public override void Crawl(string url, string? cookie, string? userAgent)
    {
        throw new NotImplementedException();
    }
}