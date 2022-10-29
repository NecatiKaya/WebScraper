using Flurl.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using WebScraper.Api.Data.Models;

namespace WebScraper.Api.HttpClients;

public class FlirlHttpClient
{
    public readonly WebScraperDbContext DbContext;

    public FlirlHttpClient(WebScraperDbContext dbContext)
    {
        DbContext = dbContext;
    }

    public async Task<string?> DownloadPageAsStringAsAsync(string? url, int? productId = null, CancellationToken cancellationToken = default(CancellationToken))
    {
        if (url is null)
        {
            return null;
        }

        try
        {
            IFlurlResponse response = await url.GetAsync(cancellationToken, HttpCompletionOption.ResponseContentRead);
            response.ResponseMessage.EnsureSuccessStatusCode();
            string html = await response.GetStringAsync();
            return html;
        }
        catch (FlurlHttpTimeoutException timeoutEx)
        {
            HttpError error = await GetHttpError(timeoutEx, productId, true);
            DbContext.HttpErrorLogs.Add(error);
            await DbContext.SaveChangesAsync();
        }
        catch (FlurlHttpException httpEx)
        {
            HttpError error = await GetHttpError(httpEx, productId, false);
            DbContext.HttpErrorLogs.Add(error);
            await DbContext.SaveChangesAsync();
        }

        return null;
    }

    private async Task<HttpError> GetHttpError(FlurlHttpException httpEx, int? productId = null, bool isTimeoutEx = false)
    {

        HttpError error = new HttpError()
        {
            Duration = httpEx.Call.Duration ?? (httpEx.Call.StartedUtc - DateTime.Now.ToUniversalTime()),
            ErrorUrl = httpEx.Call.Request.Url,
            IsTimeoutEx = isTimeoutEx,
            ProductId = productId,
            ResponseHtml = await httpEx.GetResponseStringAsync(),
            StatusCode = httpEx.StatusCode,
            ErrorDate = DateTime.Now,
            HeadersAsString = GetResponseHeadersAsString(httpEx.Call.HttpResponseMessage.Headers)
        };
        
        return error;
    }

    private string? GetResponseHeadersAsString(HttpResponseHeaders? headers)
    {
        if (headers is null)
        {
            return null;
        }

        string? headersAsString = null;
        JsonSerializerOptions options = new JsonSerializerOptions();
        options.AllowTrailingCommas = false;
        options.NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.WriteAsString;
        options.WriteIndented = false;
        headersAsString = JsonSerializer.Serialize(headers, options);
        return headersAsString;
    }

    private string? GetRequestHeadersAsString(HttpResponseHeaders? headers)
    {
        if (headers is null)
        {
            return null;
        }

        string? headersAsString = null;
        JsonSerializerOptions options = new JsonSerializerOptions();
        options.AllowTrailingCommas = false;
        options.NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.WriteAsString;
        options.WriteIndented = false;
        headersAsString = JsonSerializer.Serialize(headers, options);
        return headersAsString;
    }
}
