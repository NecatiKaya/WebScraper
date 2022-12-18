namespace WebScraper.Api.V2.HttpClients;
public abstract class CrawlerHttpClientBase
{
    public CrawlerHttpClientBase(ClientConfiguration options)
    {
        Options = options;
    }

    public ClientConfiguration Options { get; }

    public string? RequestId { get; set; }

    public virtual string Crawl(string url)
    {
        OnPreCrawl();
        return Crawl(url, null, null);
    }

    public abstract string Crawl(string url, string? cookie, string? userAgent);

    public virtual Task<string> CrawlAsync(string url)
    {
        return CrawlAsync(url, null, null);
    }

    public abstract Task<string> CrawlAsync(string url, string? cookie, string? userAgent);

    public abstract void Configure();

    public abstract Task ConfigureAsync();

    private void OnPreCrawl()
    {
        RequestId = DateTime.Now.Ticks.ToString();
    }
}
