using Flurl;
using Flurl.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using WebScraper.Api.Business;
using WebScraper.Api.Data.Models;
using WebScraper.Api.Exceptions;
using WebScraper.Api.HttpClients.PuppeteerClient;
using WebScraper.Api.Utilities;

namespace WebScraper.Api.HttpClients;

public class FlurlHttpClient
{
    public readonly WebScraperDbContext DbContext;

    public FlurlHttpClient(WebScraperDbContext dbContext)
    {
        DbContext = dbContext;
    }

    public async Task<string?> DownloadPageAsStringAsAsync(string? url, int? productId = null, UserAgentString? ua = null, CookieStore? cookieStore = null, CancellationToken cancellationToken = default(CancellationToken), int? retryCount = 0)
    {
        if (url is null)
        {
            return null;
        }

        try
        {
            RepositoryBusiness repositoryBusiness = new RepositoryBusiness(new WebScraperDbContext());
            IFlurlRequest req = new FlurlRequest(url);
            if (ua is not null)
            {
                req = url.WithHeader("user-agent", ua.Agent)
                    .WithHeader("User-Agent", ua.Agent);
            }

            if (cookieStore is not null)
            {
                req = url.WithHeader("cookie", cookieStore.CookieValue);
            }

            if (url.Contains("amazon.com.tr"))
            {
                req = url
                    .WithHeader("accept-language", "tr-TR,tr;q=0.9,en-US;q=0.8,en;q=0.7")
                    .WithHeader("Accept-Language", "tr-TR,tr;q=0.9,en-US;q=0.8,en;q=0.7")
                    .WithHeader("pragma", "no-cache")
                    .WithHeader("sec-ch-ua-platform", "Windows")
                    .WithHeader("upgrade-insecure-requests", "1")
                    .WithHeader("ect", "4g")
                    .WithHeader("cache-control", "no-cache");
            }
            IFlurlResponse response = await req.GetAsync(cancellationToken, HttpCompletionOption.ResponseContentRead);
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
               
                UserAgentString newUa = await repositoryBusiness.GetRandomUserAgent();
                CookieStore? store = await repositoryBusiness.GetNotUsedCookie(Websites.Amazon);
                if (retryCount <= 3)
                {
                    retryCount++;
                    return await DownloadPageAsStringAsAsync(url, productId, newUa, store, cancellationToken, retryCount);
                }
                else
                {
                    await LogHelper.SaveLog(LogLevel.Error, productId, url, "Retry 10 times but no response", html, GetResponseHeadersAsString(response.ResponseMessage?.Headers), 1001);
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
