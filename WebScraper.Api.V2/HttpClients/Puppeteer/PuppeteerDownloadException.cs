using WebScraper.Api.V2.Exceptions;
using WebScraper.Api.V2.Models;

namespace WebScraper.Api.V2.HttpClients.Puppeteer; 

public class PuppeteerDownloadException : WebScrapingException
{
    public PuppeteerDownloadException() : base(V2.Data.Models.Websites.Amazon, (int)ErrorCodes.PuppeteerDownloadError, "https://www.amazon.com.tr/")
    {

    }

    public PuppeteerDownloadException(Exception inner) : base(inner, V2.Data.Models.Websites.Amazon, (int)ErrorCodes.PuppeteerDownloadError, "https://www.amazon.com.tr/")
    {

    }
}
