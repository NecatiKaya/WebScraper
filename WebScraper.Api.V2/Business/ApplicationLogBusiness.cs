using Flurl.Http;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using WebScraper.Api.V2.Data.Models;
using WebScraper.Api.V2.Exceptions;
using WebScraper.Api.V2.Repositories;
using static System.Net.WebRequestMethods;

namespace WebScraper.Api.V2.Business;

public class ApplicationLogBusiness
{
    private readonly ApplicationLogRepository _applicationLogRepository;

    public ApplicationLogBusiness(ApplicationLogRepository applicationLogRepository)
    {
        _applicationLogRepository = applicationLogRepository;
    }

    public async Task AddErrorAsync(ApplicationLog log)
    {
        await _applicationLogRepository.AddAsync(log);
    }

    public static ApplicationLog CreateErrorLogFromException(string description, string jobName, string jobId, string? transactionId, Exception ex,
        DateTime? operationStartDate = null, DateTime? errorDate = null, string? url = null, string? requestId = null,
        int? productId = null, string? responseHtml = null, string? requestHeaders = null, string? responseHeaders = null,
         int? visitId = null, int? statusCode = null, bool isTimeoutEx = false
        )
    {
        DateTime nowDate = DateTime.Now;
        ApplicationLog errorLog = new ApplicationLog()
        {
            JobName = jobName,
            JobId = jobId.ToString(),
            Date = operationStartDate ?? errorDate ?? nowDate,
            Description = description,
            Level = LogLevel.Error,
            ErrorMessage = ex.Message,
            ExceptionType = ex.GetType().Name,
            StackTrace = ex.StackTrace,
            TransactionId = transactionId,
            ProductId = productId,
            Url = url,
            ResponseHtml = responseHtml,
            RequestHeadersAsString = requestHeaders,
            ResponseHeadersAsString = responseHeaders,
            ScraperVisitId = visitId,
            RequestId = requestId,
            IsTimeoutEx = isTimeoutEx
        };

        if (operationStartDate is not null)
        {
            TimeSpan? duration;
            if (errorDate is not null)
            {
                duration = errorDate - operationStartDate;
            }
            else
            {
                duration = nowDate - operationStartDate;
            }

            errorLog.Duration = duration;
        }

        if (ex is WebScrapingException)
        {
            WebScrapingException? _ex = ex as WebScrapingException;
            if (_ex is not null)
            {
                if (url is null)
                {
                    errorLog.Url = _ex.Url;
                }
                if (productId is null)
                {
                    errorLog.ProductId = _ex.ProductId;
                }
                if (responseHeaders is null)
                {
                    errorLog.ResponseHeadersAsString = ConvertToJson(_ex.HttpResponseHeaders);
                }
                if (requestHeaders is null)
                {
                    errorLog.RequestHeadersAsString = ConvertToJson(_ex.HttpRequestHeaders);
                }
                if (responseHtml is null)
                {
                    errorLog.ResponseHtml = _ex.HttpResponse;
                }
                if (statusCode is null)
                {
                    errorLog.StatusCode = _ex.StatusCode;
                }
                if (visitId is null)
                {
                    errorLog.ScraperVisitId = _ex.ScraperVisitId;
                }
            }
        }
        else if (ex is FlurlHttpException)
        {
            FlurlHttpException? httpEx = ex as FlurlHttpException;
            if (errorLog.Duration is null) 
            {
                errorLog.Duration = httpEx?.Call.Duration ?? (httpEx?.Call.StartedUtc - DateTime.Now.ToUniversalTime());
            }
            if (url is null)
            {
                errorLog.Url = httpEx?.Call?.Request?.Url;
            }
            errorLog.IsTimeoutEx = ex is FlurlHttpTimeoutException;
            if (statusCode is null)
            {
                errorLog.StatusCode = httpEx?.StatusCode;
            }
        }

        return errorLog;
    }

    public static async Task<ApplicationLog> CreateErrorLogFromExceptionAsync(string description, string jobName, string jobId, string? transactionId, Exception ex,
       DateTime? operationStartDate = null, DateTime? errorDate = null, string? url = null, string? requestId = null,
       int? productId = null, string? responseHtml = null, string? requestHeaders = null, string? responseHeaders = null,
       int? visitId = null, int? statusCode = null
       )
    {
        DateTime nowDate = DateTime.Now;
        ApplicationLog errorLog = new ApplicationLog()
        {
            JobName = jobName,
            JobId = jobId.ToString(),
            Date = operationStartDate ?? errorDate ?? nowDate,
            Description = description,
            Level = LogLevel.Error,
            ErrorMessage = ex.Message,
            ExceptionType = ex.GetType().Name,
            StackTrace = ex.StackTrace,
            TransactionId = transactionId,
            ProductId = productId,
            Url = url,
            ResponseHtml = responseHtml,
            RequestHeadersAsString = requestHeaders,
            ResponseHeadersAsString = responseHeaders,
            ScraperVisitId = visitId,
            RequestId = requestId
        };

        if (operationStartDate is not null)
        {
            TimeSpan? duration;
            if (errorDate is not null)
            {
                duration = errorDate - operationStartDate;
            }
            else
            {
                duration = nowDate - operationStartDate;
            }

            errorLog.Duration = duration;
        }

        if (ex is WebScrapingException)
        {
            WebScrapingException? _ex = ex as WebScrapingException;
            if (_ex is not null)
            {
                if (url is null)
                {
                    errorLog.Url = _ex.Url;
                }
                if (productId is null)
                {
                    errorLog.ProductId = _ex.ProductId;
                }
                if (responseHeaders is null)
                {
                    errorLog.ResponseHeadersAsString = ConvertToJson(_ex.HttpResponseHeaders);
                }
                if (requestHeaders is null)
                {
                    errorLog.RequestHeadersAsString = ConvertToJson(_ex.HttpRequestHeaders);
                }
                if (responseHtml is null)
                {
                    errorLog.ResponseHtml = _ex.HttpResponse;
                }
                if (statusCode is null)
                {
                    errorLog.StatusCode = _ex.StatusCode;
                }
                if (visitId is null)
                {
                    errorLog.ScraperVisitId = _ex.ScraperVisitId;
                }                
            }         
        }
        else if (ex is FlurlHttpException)
        {
            bool isTimeoutEx = ex is FlurlHttpTimeoutException;
            FlurlHttpException? httpEx = ex as FlurlHttpException;
            if (httpEx != null) 
            {
                if (errorLog.Duration is null)
                {
                    errorLog.Duration = httpEx.Call.Duration ?? (httpEx?.Call.StartedUtc - DateTime.Now.ToUniversalTime());
                }
                if (errorLog.Url is null)
                {
                    errorLog.Url = httpEx!.Call.Request?.Url;
                }                
                errorLog.IsTimeoutEx = isTimeoutEx;
                if (responseHtml is null)
                {
                    errorLog.ResponseHtml = !isTimeoutEx ? await httpEx!.GetResponseStringAsync() : null;
                }
                if (statusCode is null)
                {
                    errorLog.StatusCode = httpEx!.StatusCode;
                }
                if (requestHeaders is null)
                {
                    errorLog.RequestHeadersAsString = !isTimeoutEx ? GetRequestHeadersAsString(httpEx?.Call?.HttpRequestMessage?.Headers) : null; 
                }
                if (responseHeaders is null)
                {
                    errorLog.ResponseHeadersAsString = !isTimeoutEx ? GetResponseHeadersAsString(httpEx?.Call?.HttpResponseMessage?.Headers) : null;
                }
            }
        }

        return errorLog;
    }

    public static ApplicationLog CreateInformationLog(string description, string jobName, string jobId, string? transactionId, DateTime date, TimeSpan? duration = null, string? url = null, int? productId = null, string? requestId = null, string? html = null, string? requestHeaders = null, string? responseHeaders = null, int? visitId = null, int? statusCode = null)
    {
        ApplicationLog appLog = new ApplicationLog();
        appLog.JobName = jobName;
        appLog.JobId = jobId.ToString();
        appLog.Date = date;
        appLog.Description = description;
        appLog.Level = LogLevel.Information;
        appLog.Duration = duration;
        appLog.TransactionId = transactionId;
        appLog.Url = url;
        appLog.ProductId = productId;
        appLog.RequestId = requestId;
        appLog.RequestHeadersAsString = requestHeaders;
        appLog.ResponseHeadersAsString = responseHeaders;
        appLog.ResponseHtml = html;
        appLog.ScraperVisitId = visitId;
        appLog.StatusCode = statusCode;
        return appLog;
    }

    private static string? ConvertToJson(object data)
    {
        if (data is null)
        {
            return null;
        }

        JsonSerializerOptions options = new JsonSerializerOptions();
        options.AllowTrailingCommas = false;
        options.NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.WriteAsString;
        options.WriteIndented = false;
        string? headersAsString = JsonSerializer.Serialize(data, options);
        return headersAsString;
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

    private static string? GetRequestHeadersAsString(HttpRequestHeaders? headers)
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