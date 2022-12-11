using WebScraper.Api.V2.Data.Models;

namespace WebScraper.Api.V2.Exceptions;

public class TooManyRequestException : WebScrapingException
{
    public TooManyRequestException(Websites website, int statusCode, string url, int? productId = null, int? visitId = null, string? httpResponse = null, List<KeyValuePair<string, string>>? requestHeaders = null, List<KeyValuePair<string, string>>? responseHeaders = null) : base(website, statusCode, url, productId, visitId, httpResponse, requestHeaders, responseHeaders)
    {

    }
}
