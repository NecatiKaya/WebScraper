namespace WebScraper.Api.V2.Data.Models;

public enum PriceNotFoundReasons
{
    Initial = 0,
    PriceIsNotOnThePage = 1,
    ExceptionOccured = 2,
    BotDetected = 3,
    TooManyRequest = 4,
    Found = 5,
}