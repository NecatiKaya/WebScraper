using WebScraper.Api.V2.Exceptions;
using WebScraper.Api.V2.Models;

namespace WebScraper.Api.V2.HttpClients.Puppeteer; 

public class PuppeteerDirectoryAccessException : WebScrapingException
{
    public PuppeteerDirectoryAccessException() : base(V2.Data.Models.Websites.Amazon, (int)ErrorCodes.PuppeteerDirectoryAccessError, "https://www.amazon.com.tr/")
    {

    }

    public PuppeteerDirectoryAccessException(Exception inner) : base(inner, V2.Data.Models.Websites.Amazon, (int)ErrorCodes.PuppeteerDirectoryAccessError, "https://www.amazon.com.tr/")
    {

    }
}
