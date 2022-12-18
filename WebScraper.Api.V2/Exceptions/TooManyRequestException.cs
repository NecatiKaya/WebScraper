using WebScraper.Api.V2.Data.Models;
using WebScraper.Api.V2.Models;

namespace WebScraper.Api.V2.Exceptions;

public class TooManyRequestException : WebScrapingException
{
    public TooManyRequestException(Websites website, string url, int? productId = null, int? visitId = null, string? httpResponse = null, List<KeyValuePair<string, string>>? requestHeaders = null, List<KeyValuePair<string, string>>? responseHeaders = null) : base(website, (int)ErrorCodes.TooManyRequest, url, productId, visitId, httpResponse, requestHeaders, responseHeaders)
    {

    }
}
