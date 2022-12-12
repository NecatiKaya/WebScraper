namespace WebScraper.Api.V2.HttpClients
{
    public abstract class CrawlerHttpClientBase
    {
        public virtual void Crawl(string url)
        {
            Crawl(url, null, null);
        }

        public abstract void Crawl(string url, string? cookie, string? userAgent);
    }
}
