using WebScraper.Api.V2.Data.Models;

namespace WebScraper.Api.V2.Exceptions;
public class WebScrapingException : Exception
{
    public int StatusCode { get; set; }

    public string? Url { get; set; }

    public string? HttpResponse { get; set; }

    public List<KeyValuePair<string, string>> HttpResponseHeaders { get; set; } = new List<KeyValuePair<string, string>>();

    public List<KeyValuePair<string, string>> HttpRequestHeaders { get; set; } = new List<KeyValuePair<string, string>>();

    public Websites Website { get; set; }

    public int? ProductId { get; set; }

    public int? ScraperVisitId { get; set; }

    public WebScrapingException(Websites website, int statusCode, string url, int? productId = null, int? visitId = null, string? httpResponse = null, List<KeyValuePair<string, string>>? requestHeaders = null,
        List<KeyValuePair<string, string>>? responseHeaders = null) : base($"From {website}: WebScrapingException occured for url ('{url}') with status code {statusCode}.")
    {
        Website = website;
        StatusCode = statusCode;
        Url = url;
        HttpRequestHeaders = requestHeaders ?? new List<KeyValuePair<string, string>>();
        HttpResponseHeaders = responseHeaders ?? new List<KeyValuePair<string, string>>();
        HttpResponse = httpResponse;
        ProductId = productId;
        ScraperVisitId = visitId;
    }

    public WebScrapingException(Exception innerException, Websites website, int statusCode, string url, int? productId = null, int? visitId = null, string? httpResponse = null, List<KeyValuePair<string, string>>? requestHeaders = null,
        List<KeyValuePair<string, string>>? responseHeaders = null) : base($"From {website}: WebScrapingException occured for url ('{url}') with status code {statusCode}.", innerException)
    {
        Website = website;
        StatusCode = statusCode;
        Url = url;
        HttpRequestHeaders = requestHeaders ?? new List<KeyValuePair<string, string>>();
        HttpResponseHeaders = responseHeaders ?? new List<KeyValuePair<string, string>>();
        HttpResponse = httpResponse;
        ProductId = productId;
        ScraperVisitId = visitId;
    }
}