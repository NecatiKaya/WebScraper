using WebScraper.Api.V2.Data.Models;

namespace WebScraper.Api.V2.HttpClients;

public abstract class CrawlerHttpClientBase
{
    private string _requestId = DateTime.Now.Ticks.ToString();

    public CrawlerHttpClientBase(ClientConfiguration options)
    {
        Options = options;
    }

    public ClientConfiguration Options { get; }

    public virtual HttpClientResponse? Crawl(Product product)
    {
        OnPreCrawl();
        return Crawl(product, null, null);
    }

    public abstract HttpClientResponse? Crawl(Product product, string? cookie, string? userAgent);

    public virtual Task<HttpClientResponse?> CrawlAsync(Product product, CancellationToken cancellationToken = default)
    {
        OnPreCrawl();
        return CrawlAsync(product, null, null, cancellationToken);
    }

    public abstract Task<HttpClientResponse?> CrawlAsync(Product product, string? cookie, string? userAgent, CancellationToken cancellationToken = default);

    public abstract void Configure();

    public abstract Task ConfigureAsync();
    
    public string GetRequestId()
    {
        return _requestId;
    }

    private void OnPreCrawl()
    {
        _requestId = DateTime.Now.Ticks.ToString();
    }
}
