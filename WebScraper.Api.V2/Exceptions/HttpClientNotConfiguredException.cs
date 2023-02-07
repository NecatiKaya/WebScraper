using WebScraper.Api.V2.Data.Models;
using WebScraper.Api.V2.Models;

namespace WebScraper.Api.V2.Exceptions;

public class HttpClientNotConfiguredException : WebScrapingException
{
    public HttpClientNotConfiguredException(Websites website, string? url = null, int? productId = null, int? visitId = null, string? httpResponse = null, List<KeyValuePair<string, string>>? requestHeaders = null, List<KeyValuePair<string, string>>? responseHeaders = null) : base(website, (int)ErrorCodes.HttpClientNotConfigured, url, productId, visitId, httpResponse, requestHeaders, responseHeaders)
    {

    }
}
