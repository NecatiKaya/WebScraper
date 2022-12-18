using WebScraper.Api.V2.Data.Models;
using WebScraper.Api.V2.Models;

namespace WebScraper.Api.V2.Exceptions;

public class AmazonBotDetectedException : WebScrapingException
{
    public AmazonBotDetectedException(string url, int? productId = null, int? visitId = null, string? httpResponse = null, List<KeyValuePair<string, string>>? requestHeaders = null,
        List<KeyValuePair<string, string>>? responseHeaders = null) : base(Websites.Amazon, (int)ErrorCodes.AmazonBotDetected, url, productId, visitId, httpResponse, requestHeaders, responseHeaders)
    {
        Url = url;
        HttpRequestHeaders = requestHeaders ?? new List<KeyValuePair<string, string>>();
        HttpResponseHeaders = responseHeaders ?? new List<KeyValuePair<string, string>>();
        HttpResponse = httpResponse;
    }
}