using WebScraper.Api.V2.Exceptions;
using WebScraper.Api.V2.Models;

namespace WebScraper.Api.V2.HttpClients.Puppeteer;

public class PuppeteerCrawlCookieException : WebScrapingException
{
    public PuppeteerCrawlCookieException() : base(V2.Data.Models.Websites.Amazon, (int)ErrorCodes.PuppeteerCrawlCookieError, "https://www.amazon.com.tr/")
    {

    }
}
