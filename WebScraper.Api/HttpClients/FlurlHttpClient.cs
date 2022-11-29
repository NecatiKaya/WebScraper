using Flurl.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using WebScraper.Api.Data.Models;
using WebScraper.Api.Utilities;

namespace WebScraper.Api.HttpClients;

public class FlurlHttpClient
{
    public readonly WebScraperDbContext DbContext;

    public FlurlHttpClient(WebScraperDbContext dbContext)
    {
        DbContext = dbContext;
    }

    public async Task<string?> DownloadPageAsStringAsAsync(string? url, int? productId = null, UserAgentString? ua = null, CancellationToken cancellationToken = default(CancellationToken))
    {
        if (url is null)
        {
            return null;
        }

        try
        {
            IFlurlResponse response = null;
            if (ua is null)
            {
                response = await url.GetAsync(cancellationToken, HttpCompletionOption.ResponseContentRead);
            }
            else
            {
                response = await url
                    //.WithHeader("accept-language", "tr-TR,tr;q=0.9,en-US;q=0.8,en;q=0.7")
                    //.WithHeader("user-agent", ua.Agent)
                    .GetAsync(cancellationToken, HttpCompletionOption.ResponseContentRead);
            }
            
            response.ResponseMessage.EnsureSuccessStatusCode();
            string html = await response.GetStringAsync();

            if (url.Contains("amazon.com.tr") && html.ToLower().Contains("sadece robot olma") == true)
            {
                using (WebScraperDbContext ctx = new WebScraperDbContext())
                {
                    HttpError error = new HttpError();
                    error.ErrorUrl = url;
                    error.ProductId = productId;
                    error.Duration = null;
                    error.IsTimeoutEx = false;
                    error.ResponseHtml = html;
                    error.StatusCode = -1000;
                    error.ErrorDate = DateTime.Now;
                    error.HeadersAsString = GetResponseHeadersAsString(response.ResponseMessage?.Headers);
                    ctx.HttpErrorLogs.Add(error);
                    await ctx.SaveChangesAsync();
                }
            }

            return html;
        }
        catch (FlurlHttpTimeoutException timeoutEx)
        {
            ////DbContext.HttpErrorLogs.Add(error);
            ////DbContext.SaveChanges();

            //using (WebScraperDbContext ctx = new WebScraperDbContext())
            //{
            //    HttpError error = await GetHttpError(timeoutEx, productId, true);
            //    ctx.HttpErrorLogs.Add(error);
            //    await ctx.SaveChangesAsync();
            //}

            await LogHelper.SaveHttpErrorLog(productId, timeoutEx);
        }
        catch (FlurlHttpException httpEx)
        {
            ////DbContext.HttpErrorLogs.Add(error);
            ////DbContext.SaveChanges();        

            //using (WebScraperDbContext ctx = new WebScraperDbContext())
            //{
            //    HttpError error = await GetHttpError(httpEx, productId, true);
            //    ctx.HttpErrorLogs.Add(error);
            //    await ctx.SaveChangesAsync();
            //}

            await LogHelper.SaveHttpErrorLog(productId, httpEx);
        }

        return null;
    }

    //private async Task<HttpError> GetHttpError(FlurlHttpException httpEx, int? productId = null, bool isTimeoutEx = false)
    //{
    //    HttpError error = new HttpError(); 
    //    error.Duration = httpEx?.Call.Duration ?? (httpEx?.Call.StartedUtc - DateTime.Now.ToUniversalTime());
    //    error.ErrorUrl = httpEx?.Call?.Request?.Url;
    //    error.IsTimeoutEx = isTimeoutEx;
    //    error.ProductId = productId;
    //    error.ResponseHtml = httpEx != null ? await httpEx.GetResponseStringAsync() : null;
    //    error.StatusCode = httpEx?.StatusCode;
    //    error.ErrorDate = DateTime.Now;
    //    error.HeadersAsString = GetResponseHeadersAsString(httpEx?.Call?.HttpResponseMessage?.Headers);
    //    return error;
    //}

    private string? GetResponseHeadersAsString(HttpResponseHeaders? headers)
    {
        if (headers is null)
        {
            return null;
        }

        JsonSerializerOptions options = new JsonSerializerOptions();
        options.AllowTrailingCommas = false;
        options.NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.WriteAsString;
        options.WriteIndented = false;
        string? headersAsString = JsonSerializer.Serialize(headers, options);
        return headersAsString;
    }
}
