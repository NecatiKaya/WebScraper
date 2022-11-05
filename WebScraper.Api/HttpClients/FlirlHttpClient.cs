using Flurl.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
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
            //DbContext.HttpErrorLogs.Add(error);
            //DbContext.SaveChanges();

            using (WebScraperDbContext ctx = new WebScraperDbContext())
            {
                HttpError error = await GetHttpError(timeoutEx, productId, true);
                ctx.HttpErrorLogs.Add(error);
                await ctx.SaveChangesAsync();
            }            
        }
        catch (FlurlHttpException httpEx)
        {
            //DbContext.HttpErrorLogs.Add(error);
            //DbContext.SaveChanges();        

            using (WebScraperDbContext ctx = new WebScraperDbContext())
            {
                HttpError error = await GetHttpError(httpEx, productId, true);
                ctx.HttpErrorLogs.Add(error);
                await ctx.SaveChangesAsync();
            }
        }

        return null;
    }

    private async Task<HttpError> GetHttpError(FlurlHttpException httpEx, int? productId = null, bool isTimeoutEx = false)
    {
        HttpError error = new HttpError(); 
        error.Duration = httpEx?.Call.Duration ?? (httpEx?.Call.StartedUtc - DateTime.Now.ToUniversalTime());
        error.ErrorUrl = httpEx?.Call?.Request?.Url;
        error.IsTimeoutEx = isTimeoutEx;
        error.ProductId = productId;
        error.ResponseHtml = httpEx != null ? await httpEx.GetResponseStringAsync() : null;
        error.StatusCode = httpEx?.StatusCode;
        error.ErrorDate = DateTime.Now;
        error.HeadersAsString = GetResponseHeadersAsString(httpEx?.Call?.HttpResponseMessage?.Headers);
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
}
