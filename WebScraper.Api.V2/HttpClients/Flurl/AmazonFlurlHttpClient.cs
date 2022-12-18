using Flurl.Http;
using Flurl.Http.Configuration;

namespace WebScraper.Api.V2.HttpClients.Flurl;

public class AmazonFlurlHttpClient : CrawlerHttpClientBase
{
    private readonly IFlurlClient _flurlClient;

    public AmazonFlurlHttpClient(IFlurlClientFactory flurlClientFac, ClientConfiguration clientConfiguration) : base(clientConfiguration)
    {
        _flurlClient = flurlClientFac.Get("https://www.amazon.com.tr/");
    }

    public override void Configure()
    {
        throw new NotImplementedException();
    }

    public override Task ConfigureAsync()
    {
        throw new NotImplementedException();
    }

    public override string Crawl(string url, string? cookie, string? userAgent)
    {
        throw new NotImplementedException("AmazonFlurlHttpClient.Crawl() is not implemented. Please make use of one of CrawlAsync(...) methods");
    }

    public override Task<string> CrawlAsync(string url, string? cookie, string? userAgent)
    {
        throw new NotImplementedException();
    }
}