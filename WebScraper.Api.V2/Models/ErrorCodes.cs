namespace WebScraper.Api.V2.Models;
public enum ErrorCodes
{
    /* Web Client Errors[ */
    AmazonBotDetected = -1000,
    AmazonBotDetectedAndRetry = -1001,
    TooManyRequest = -2000,
    /* ]Web Client Errors */

    /* Puppeteer Errors[ */
    PuppeteerCrawlCookieError = -3000,
    PuppeteerDirectoryAccessError = -3001,
    PuppeteerDownloadError = 3002,
    PuppeteerExecutablePathError = 3003
    /* ]Puppeteer Errors */
}
