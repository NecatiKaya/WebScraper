using WebScraper.Api.V2.Exceptions;
using WebScraper.Api.V2.Models;

namespace WebScraper.Api.V2.HttpClients.Puppeteer;

public class PuppeteerExecutablePathException : WebScrapingException
{
    public PuppeteerExecutablePathException() : base(V2.Data.Models.Websites.Amazon, (int)ErrorCodes.PuppeteerExecutablePathError, "https://www.amazon.com.tr/")
    {

    }
}