using WebScraper.Api.V2.Data.Models;

namespace WebScraper.Api.V2.Exceptions;

public class AmazonBotDetectedException : WebScrapingException
{
    public AmazonBotDetectedException(int statusCode, string url, int? productId = null, int? visitId = null, string? httpResponse = null, List<KeyValuePair<string, string>>? requestHeaders = null,
        List<KeyValuePair<string, string>>? responseHeaders = null) : base(Websites.Amazon, statusCode, url, productId, visitId, httpResponse, requestHeaders, responseHeaders)
    {
        StatusCode = statusCode;
        Url = url;
        HttpRequestHeaders = requestHeaders ?? new List<KeyValuePair<string, string>>();
        HttpResponseHeaders = responseHeaders ?? new List<KeyValuePair<string, string>>();
        HttpResponse = httpResponse;
    }
}