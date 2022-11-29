using Flurl.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using WebScraper.Api.Data.Models;

namespace WebScraper.Api.Utilities;

public static class LogHelper
{
    public static async Task SaveLog(LogLevel logLevel = LogLevel.Information, int ? productId = null, 
        string? url = null, string? message = null, 
        string? responseHtml = null, string? responseHeaders = null,
        int? httpStatus = 200)
    {
        AppLog log = new AppLog();
        log.Date = DateTime.Now;
        log.ProductId = productId;
        log.Description = message;
        log.Level = logLevel;
        log.Url = url;
        log.ResponseHtml = responseHtml;
        log.HeadersAsString = responseHeaders;
        log.StatusCode = httpStatus;
        await SaveToDatabase(log);
    }

    public static async Task SaveInformationLog(int? productId = null, string? message = null, string? jobName = null)
    {
        AppLog log = new AppLog();
        log.Date = DateTime.Now;
        log.ProductId = productId;
        log.Description = message;
        log.JobName = jobName;
        await SaveToDatabase(log);
    }

    public static async Task SaveErrorLog(Exception ex, int? productId = null, string? url = null, string? message = null, string? responseHtml = null, string? responseHeaders = null,
        int? httpStatus = 1000)
    {
        AppLog log = new AppLog();
        log.Date = DateTime.Now;
        log.ProductId = productId;
        log.StackTrace = ex.StackTrace;
        log.ErrorMessage = ex.Message;
        log.Level = LogLevel.Error;
        log.Description = message;
        log.Url = url;
        log.ResponseHtml = responseHtml;
        log.HeadersAsString = responseHeaders;
        log.StatusCode = httpStatus;
        await SaveToDatabase(log);
    }

    public static async Task SaveHttpErrorLog(int? productId, FlurlHttpException ex)
    {
        using (WebScraperDbContext ctx = new WebScraperDbContext())
        {
            HttpError error = null;
            if (ex is FlurlHttpTimeoutException)
            {
                error = await GetHttpError(((FlurlHttpTimeoutException)ex) , productId, true);
            }
            else
            {
                error = await GetHttpError(ex, productId, false);
            }
            
            ctx.HttpErrorLogs.Add(error);
            await ctx.SaveChangesAsync();
        }
    }

    private static async Task<HttpError> GetHttpError(FlurlHttpException httpEx, int? productId = null, bool isTimeoutEx = false)
    {
        HttpError error = new HttpError();
        error.Duration = httpEx?.Call.Duration ?? (httpEx?.Call.StartedUtc - DateTime.Now.ToUniversalTime());
        error.ErrorUrl = httpEx?.Call?.Request?.Url;
        error.IsTimeoutEx = isTimeoutEx;
        error.ProductId = productId;
        error.ResponseHtml = httpEx != null ? await httpEx.GetResponseStringAsync() : null;
        error.StatusCode = httpEx?.StatusCode;
        error.ErrorDate = DateTime.Now;
        error.HeadersAsString = !isTimeoutEx ? GetResponseHeadersAsString(httpEx?.Call?.HttpResponseMessage?.Headers) : null;
        return error;
    }

    private static string? GetResponseHeadersAsString(HttpResponseHeaders? headers)
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

    private static async Task SaveToDatabase(AppLog appLog)
    {
        using (WebScraperDbContext ctx = new WebScraperDbContext())
        {
            ctx.AppLogs.Add(appLog);
            await ctx.SaveChangesAsync();
        }
    }
}