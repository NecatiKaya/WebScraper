using Flurl.Http;
using Flurl.Http.Configuration;

namespace WebScraper.Api.V2.HttpClients.Flurl;

public class TrendyolFlurlHttpClient : CrawlerHttpClientBase
{
    private readonly IFlurlClient _flurlClient;

    public TrendyolFlurlHttpClient(IFlurlClientFactory flurlClientFac)
    {
        _flurlClient = flurlClientFac.Get("https://www.trendyol.com/");
    }
    public override void Crawl(string url, string? cookie, string? userAgent)
    {
        throw new NotImplementedException();
    }
}
